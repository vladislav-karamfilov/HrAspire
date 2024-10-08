namespace HrAspire.Employees.Business.Documents;

using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Sas;

using HeyRed.Mime;

using HrAspire.Business.Common;
using HrAspire.Employees.Business.Mappers;
using HrAspire.Employees.Data;
using HrAspire.Employees.Data.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using StackExchange.Redis;

public class DocumentsService : IDocumentsService
{
    private readonly EmployeesDbContext dbContext;
    private readonly BlobServiceClient blobServiceClient;
    private readonly IConnectionMultiplexer cacheConnectionMultiplexer;
    private readonly TimeProvider timeProvider;
    private readonly ILogger<DocumentsService> logger;

    public DocumentsService(
        EmployeesDbContext dbContext,
        BlobServiceClient blobServiceClient,
        IConnectionMultiplexer cacheConnectionMultiplexer,
        TimeProvider timeProvider,
        ILogger<DocumentsService> logger)
    {
        this.dbContext = dbContext;
        this.blobServiceClient = blobServiceClient;
        this.cacheConnectionMultiplexer = cacheConnectionMultiplexer;
        this.timeProvider = timeProvider;
        this.logger = logger;
    }

    private IDatabase CacheDatabase => this.cacheConnectionMultiplexer.GetDatabase();

    public async Task<ServiceResult<int>> CreateAsync(
        string employeeId,
        string title,
        string? description,
        byte[] fileContent,
        string fileName,
        string createdById)
    {
        if (await this.IsEmployeeManagerAsync(createdById) && createdById != await this.GetEmployeeManagerIdAsync(employeeId))
        {
            return ServiceResult<int>.Error("Employee to create document for doesn't exist.");
        }

        var url = await this.UploadFileToBlobStorageAsync(fileContent, fileName, employeeId);
        if (string.IsNullOrWhiteSpace(url))
        {
            return ServiceResult<int>.Error("An error has occurred while uploading the document. Please try again later.");
        }

        var document = new Document
        {
            EmployeeId = employeeId,
            Title = title,
            Description = description,
            Url = url,
            FileName = fileName,
            CreatedOn = this.timeProvider.GetUtcNow().UtcDateTime,
            CreatedById = createdById,
        };

        this.dbContext.Documents.Add(document);
        await this.dbContext.SaveChangesAsync();

        return ServiceResult<int>.Success(document.Id);
    }

    public async Task<ServiceResult> DeleteAsync(int id, string currentEmployeeId)
    {
        var deletedCount = await this.dbContext.Documents
            .Where(d => d.Id == id && d.CreatedById == currentEmployeeId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(e => e.IsDeleted, true)
                .SetProperty(e => e.DeletedOn, this.timeProvider.GetUtcNow().UtcDateTime));

        return deletedCount > 0 ? ServiceResult.Success : ServiceResult.ErrorNotFound;
    }

    public async Task<IEnumerable<DocumentServiceModel>> ListEmployeeDocumentsAsync(
        string employeeId,
        int pageNumber,
        int pageSize,
        string? managerId)
    {
        if (!string.IsNullOrEmpty(managerId) && managerId != await this.GetEmployeeManagerIdAsync(employeeId))
        {
            return [];
        }

        PaginationHelper.Normalize(ref pageNumber, ref pageSize);

        return await this.dbContext.Documents
            .Where(d => d.EmployeeId == employeeId)
            .OrderByDescending(d => d.CreatedOn)
            .Skip(pageNumber * pageSize)
            .Take(pageSize)
            .ProjectToServiceModel()
            .ToListAsync();
    }

    public async Task<int> GetEmployeeDocumentsCountAsync(string employeeId, string? managerId)
    {
        if (!string.IsNullOrEmpty(managerId) && managerId != await this.GetEmployeeManagerIdAsync(employeeId))
        {
            return 0;
        }

        return await this.dbContext.Documents.CountAsync(d => d.EmployeeId == employeeId);
    }

    public async Task<DocumentDetailsServiceModel?> GetAsync(int id, string? managerId)
    {
        var document = await this.dbContext.Documents.Where(d => d.Id == id).ProjectToDetailsServiceModel().FirstOrDefaultAsync();
        if (document is not null &&
            !string.IsNullOrEmpty(managerId) &&
            managerId != await this.GetEmployeeManagerIdAsync(document.EmployeeId))
        {
            return null;
        }

        return document;
    }

    public async Task<DocumentUrlAndFileNameServiceModel?> GetUrlAndFileNameAsync(int id, string? managerId)
    {
        var urlAndFileNameInfo = await this.dbContext.Documents
            .Where(d => d.Id == id)
            .ProjectToUrlAndFileNameServiceModel()
            .FirstOrDefaultAsync();

        if (urlAndFileNameInfo is not null &&
            !string.IsNullOrEmpty(managerId) &&
            managerId != await this.GetEmployeeManagerIdAsync(urlAndFileNameInfo.EmployeeId))
        {
            return null;
        }

        return urlAndFileNameInfo;
    }

    public async Task<ServiceResult> UpdateAsync(
        int id,
        string title,
        string? description,
        byte[]? fileContent,
        string? fileName,
        string currentEmployeeId)
    {
        var document = await this.dbContext.Documents.FirstOrDefaultAsync(d => d.Id == id);
        if (document is null || document.CreatedById != currentEmployeeId)
        {
            return ServiceResult.ErrorNotFound;
        }

        if (fileContent is not null && !string.IsNullOrWhiteSpace(fileName))
        {
            // TODO: Consider deleting the old file from blob storage to save storage
            var url = await this.UploadFileToBlobStorageAsync(fileContent, fileName, document.EmployeeId);
            if (string.IsNullOrWhiteSpace(url))
            {
                return ServiceResult<int>.Error("An error has occurred while uploading the document. Please try again later.");
            }

            document.Url = url;
            document.FileName = fileName;
        }

        document.Title = title;
        document.Description = description;

        await this.dbContext.SaveChangesAsync();

        return ServiceResult.Success;
    }

    private async Task<string?> UploadFileToBlobStorageAsync(byte[] fileContent, string fileName, string employeeId)
    {
        var containerName = $"documents-{employeeId}";
        var blobName = $"{Guid.NewGuid()}{Path.GetExtension(fileName)}";
        var contentType = MimeTypesMap.GetMimeType(blobName);

        try
        {
            var blobClient = await this.GetBlobClientAsync(blobName, containerName);

            using var fileStream = new MemoryStream(fileContent);
            await blobClient.UploadAsync(fileStream, new BlobHttpHeaders { ContentType = contentType });
        }
        catch (Exception ex)
        {
            this.logger.LogError("Error uploading file '{File}' to container '{Container}': {Exception}", fileName, containerName, ex);
            return null;
        }

        var sasUri = this.blobServiceClient.GenerateAccountSasUri(
            AccountSasPermissions.Read,
            this.timeProvider.GetUtcNow().AddYears(10),
            AccountSasResourceTypes.Object);

        var uriBuilder = new UriBuilder(sasUri)
        {
            Path = $"{sasUri.LocalPath}/{containerName}/{blobName}",
            Port = sasUri.Port is 80 or 443 ? -1 : sasUri.Port,
        };

        return uriBuilder.ToString();
    }

    private async Task<BlockBlobClient> GetBlobClientAsync(string blobName, string containerName)
    {
        var container = this.blobServiceClient.GetBlobContainerClient(containerName);
        await container.CreateIfNotExistsAsync();

        return container.GetBlockBlobClient(blobName);
    }

    private Task<bool> IsEmployeeManagerAsync(string employeeId)
        => this.dbContext.UserRoles.AnyAsync(ur => ur.UserId == employeeId && ur.RoleId == BusinessConstants.ManagerRole);

    private async Task<string?> GetEmployeeManagerIdAsync(string employeeId)
    {
        var employeeInfo = await this.CacheDatabase.HashGetAsync(BusinessConstants.EmployeesCacheSetName, employeeId);

        var cachedEmployee = employeeInfo.IsNull ? null : JsonSerializer.Deserialize<CachedEmployee>(employeeInfo!);
        return cachedEmployee?.ManagerId;
    }
}
