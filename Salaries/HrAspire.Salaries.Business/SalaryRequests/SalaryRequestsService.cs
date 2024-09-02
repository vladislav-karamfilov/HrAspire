namespace HrAspire.Salaries.Business.SalaryRequests;

using System.Collections.Generic;
using System.Threading.Tasks;

using HrAspire.Business.Common;
using HrAspire.Salaries.Business.Mappers;
using HrAspire.Salaries.Data;
using HrAspire.Salaries.Data.Models;

using Microsoft.EntityFrameworkCore;

using StackExchange.Redis;

public class SalaryRequestsService : ISalaryRequestsService
{
    private readonly SalariesDbContext dbContext;
    private readonly IConnectionMultiplexer cacheConnectionMultiplexer;
    private readonly TimeProvider timeProvider;

    public SalaryRequestsService(
        SalariesDbContext dbContext,
        IConnectionMultiplexer cacheConnectionMultiplexer,
        TimeProvider timeProvider)
    {
        this.dbContext = dbContext;
        this.cacheConnectionMultiplexer = cacheConnectionMultiplexer;
        this.timeProvider = timeProvider;
    }

    private IDatabase CacheDatabase => this.cacheConnectionMultiplexer.GetDatabase();

    public async Task<ServiceResult<int>> CreateAsync(string employeeId, decimal newSalary, string? notes, string createdById)
    {
        if (!await this.EmployeeExistsAsync(employeeId))
        {
            return ServiceResult<int>.Error("Employee to create salary request for doesn't exist.");
        }

        // TODO:
        //if (!await this.EmployeeExistsAsync(createdById))
        //{
        //    return ServiceResult<int>.Error("Salary request creator employee doesn't exist.");
        //}

        var salaryRequest = new SalaryRequest
        {
            EmployeeId = employeeId,
            NewSalary = newSalary,
            Notes = notes,
            Status = SalaryRequestStatus.Pending,
            CreatedById = createdById,
            CreatedOn = this.timeProvider.GetUtcNow().UtcDateTime,
        };

        this.dbContext.Add(salaryRequest);
        await this.dbContext.SaveChangesAsync();

        return ServiceResult<int>.Success(salaryRequest.Id);
    }

    public async Task<ServiceResult> DeleteAsync(int id)
    {
        var deletedCount = await this.dbContext.SalaryRequests
            .Where(e => e.Id == id)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(e => e.IsDeleted, true)
                .SetProperty(e => e.DeletedOn, this.timeProvider.GetUtcNow().UtcDateTime));

        return deletedCount > 0 ? ServiceResult.Success : ServiceResult.ErrorNotFound;
    }

    public async Task<SalaryRequestDetailsServiceModel?> GetAsync(int id)
    {
        var salaryRequest = await this.dbContext.SalaryRequests
            .Where(r => r.Id == id)
            .ProjectToDetailsServiceModel()
            .FirstOrDefaultAsync();

        if (salaryRequest is null)
        {
            return null;
        }

        salaryRequest.EmployeeFullName = (await this.GetEmployeeNamesAsync(salaryRequest.EmployeeId))[0] ?? string.Empty;
        salaryRequest.CreatedByFullName = (await this.GetEmployeeNamesAsync(salaryRequest.CreatedById))[0] ?? string.Empty;

        return salaryRequest;
    }

    public async Task<IEnumerable<SalaryRequestServiceModel>> ListAsync(int pageNumber, int pageSize)
    {
        PaginationHelper.Normalize(ref pageNumber, ref pageSize);

        return await this.dbContext.SalaryRequests
            .OrderByDescending(d => d.CreatedOn)
            .Skip(pageNumber * pageSize)
            .Take(pageSize)
            .ProjectToServiceModel()
            .ToListAsync();
    }

    public Task<int> GetCountAsync() => this.dbContext.SalaryRequests.CountAsync();

    public async Task<ServiceResult> UpdateAsync(int id, decimal newSalary, string? notes)
    {
        var updatedCount = await this.dbContext.SalaryRequests
            .Where(e => e.Id == id)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(e => e.NewSalary, newSalary)
                .SetProperty(e => e.Notes, notes));

        return updatedCount > 0 ? ServiceResult.Success : ServiceResult.ErrorNotFound;
    }

    public async Task<ServiceResult> ApproveAsync(int id, string approvedById)
    {
        if (!await this.EmployeeExistsAsync(approvedById))
        {
            return ServiceResult<int>.Error("Salary request approving employee doesn't exist.");
        }

        var salaryRequest = await this.dbContext.SalaryRequests.FirstOrDefaultAsync(r => r.Id == id);
        if (salaryRequest is null)
        {
            return ServiceResult.ErrorNotFound;
        }

        if (salaryRequest.Status == SalaryRequestStatus.Approved)
        {
            return ServiceResult.Success;
        }

        if (salaryRequest.Status == SalaryRequestStatus.Rejected)
        {
            return ServiceResult.Error("Salary request has already been rejected.");
        }

        salaryRequest.Status = SalaryRequestStatus.Approved;
        salaryRequest.StatusChangedOn = this.timeProvider.GetUtcNow().UtcDateTime;
        salaryRequest.StatusChangedById = approvedById;

        // TODO: add outbox message
        await this.dbContext.SaveChangesAsync();

        return ServiceResult.Success;
    }

    public async Task<ServiceResult> RejectAsync(int id, string rejectedById)
    {
        if (!await this.EmployeeExistsAsync(rejectedById))
        {
            return ServiceResult<int>.Error("Salary request rejecting employee doesn't exist.");
        }

        var salaryRequest = await this.dbContext.SalaryRequests.FirstOrDefaultAsync(r => r.Id == id);
        if (salaryRequest is null)
        {
            return ServiceResult.ErrorNotFound;
        }

        if (salaryRequest.Status == SalaryRequestStatus.Rejected)
        {
            return ServiceResult.Success;
        }

        if (salaryRequest.Status == SalaryRequestStatus.Approved)
        {
            return ServiceResult.Error("Salary request has already been approved.");
        }

        salaryRequest.Status = SalaryRequestStatus.Rejected;
        salaryRequest.StatusChangedOn = this.timeProvider.GetUtcNow().UtcDateTime;
        salaryRequest.StatusChangedById = rejectedById;

        await this.dbContext.SaveChangesAsync();

        return ServiceResult.Success;
    }

    private Task<bool> EmployeeExistsAsync(string employeeId)
        => this.CacheDatabase.HashExistsAsync(BusinessConstants.EmployeesCacheSetName, employeeId);

    private async Task<string[]> GetEmployeeNamesAsync(params string[] employeeIds)
    {
        await Task.CompletedTask;

        // TODO:
        return new string[] { "test" };
    }
}
