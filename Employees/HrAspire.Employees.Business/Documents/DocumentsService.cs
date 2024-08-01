﻿namespace HrAspire.Employees.Business.Documents;

using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;

using HeyRed.Mime;

using HrAspire.Business.Common;
using HrAspire.Employees.Business.Mappers;
using HrAspire.Employees.Data;
using HrAspire.Employees.Data.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public class DocumentsService : IDocumentsService
{
    private readonly EmployeesDbContext dbContext;
    private readonly BlobServiceClient blobServiceClient;
    private readonly TimeProvider timeProvider;
    private readonly ILogger<DocumentsService> logger;

    public DocumentsService(
        EmployeesDbContext dbContext,
        BlobServiceClient blobServiceClient,
        TimeProvider timeProvider,
        ILogger<DocumentsService> logger)
    {
        this.dbContext = dbContext;
        this.blobServiceClient = blobServiceClient;
        this.timeProvider = timeProvider;
        this.logger = logger;
    }

    public async Task<ServiceResult<int>> CreateAsync(
        string employeeId,
        string title,
        string? description,
        byte[] fileContent,
        string fileName,
        string createdById)
    {
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

    public async Task<ServiceResult> DeleteAsync(int id, string employeeId)
    {
        var deletedCount = await this.dbContext.Documents
            .Where(d => d.Id == id && d.EmployeeId == employeeId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(e => e.IsDeleted, true)
                .SetProperty(e => e.DeletedOn, this.timeProvider.GetUtcNow().UtcDateTime));

        return deletedCount > 0 ? ServiceResult.Success : ServiceResult.ErrorNotFound;
    }

    public async Task<IEnumerable<DocumentServiceModel>> GetEmployeeDocumentsAsync(string employeeId, int pageNumber, int pageSize)
    {
        pageNumber = Math.Max(pageNumber, 0);

        if (pageSize <= 0)
        {
            pageSize = 10;
        }
        else if (pageSize > 100)
        {
            pageSize = 100;
        }

        return await this.dbContext.Documents
            .OrderByDescending(d => d.CreatedOn)
            .Skip(pageNumber * pageSize)
            .Take(pageSize)
            .ProjectToServiceModel()
            .ToListAsync();
    }

    public Task<int> GetEmployeeDocumentsCountAsync(string employeeId)
        => this.dbContext.Documents.CountAsync(d => d.EmployeeId == employeeId);

    public Task<DocumentDetailsServiceModel?> GetDocumentAsync(int id, string employeeId)
        => this.dbContext.Documents
            .Where(d => d.Id == id && d.EmployeeId == employeeId)
            .ProjectToDetailsServiceModel()
            .FirstOrDefaultAsync();

    public async Task<ServiceResult> UpdateAsync(
        int id,
        string employeeId,
        string title,
        string? description,
        byte[]? fileContent,
        string? fileName)
    {
        var document = await this.dbContext.Documents.FirstOrDefaultAsync(d => d.Id == id && d.EmployeeId == employeeId);
        if (document is null)
        {
            return ServiceResult.ErrorNotFound;
        }

        if (fileContent is not null && !string.IsNullOrWhiteSpace(fileName))
        {
            // TODO: Consider deleting the old file from blob storage
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
            this.logger.LogError("Error uploading file '{file}' to container '{container}': {exception}", fileName, containerName, ex);
            return null;
        }

        var uriBuilder = new UriBuilder(this.blobServiceClient.Uri) { Path = $"{containerName}/{blobName}", Port = -1 };
        return uriBuilder.ToString();
    }

    private async Task<BlockBlobClient> GetBlobClientAsync(string blobName, string containerName)
    {
        var container = this.blobServiceClient.GetBlobContainerClient(containerName);
        await container.CreateIfNotExistsAsync();

        return container.GetBlockBlobClient(blobName);
    }
}