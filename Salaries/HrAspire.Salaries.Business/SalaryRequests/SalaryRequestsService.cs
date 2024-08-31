namespace HrAspire.Salaries.Business.SalaryRequests;

using System.Collections.Generic;
using System.Threading.Tasks;

using HrAspire.Business.Common;
using HrAspire.Salaries.Business.Mappers;
using HrAspire.Salaries.Data;
using HrAspire.Salaries.Data.Models;

using Microsoft.EntityFrameworkCore;

public class SalaryRequestsService : ISalaryRequestsService
{
    private readonly SalariesDbContext dbContext;
    private readonly TimeProvider timeProvider;

    public SalaryRequestsService(SalariesDbContext dbContext, TimeProvider timeProvider)
    {
        this.dbContext = dbContext;
        this.timeProvider = timeProvider;
    }

    public async Task<ServiceResult<int>> CreateAsync(string employeeId, decimal newSalary, string? notes, string createdById)
    {
        // TODO: validate that employee is present

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

    public Task<SalaryRequestDetailsServiceModel?> GetSalaryRequestAsync(int id)
        => this.dbContext.SalaryRequests.Where(r => r.Id == id).ProjectToDetailsServiceModel().FirstOrDefaultAsync();

    public async Task<IEnumerable<SalaryRequestServiceModel>> GetSalaryRequestsAsync(int pageNumber, int pageSize)
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

        return await this.dbContext.SalaryRequests
            .OrderByDescending(d => d.CreatedOn)
            .Skip(pageNumber * pageSize)
            .Take(pageSize)
            .ProjectToServiceModel()
            .ToListAsync();
    }

    public Task<int> GetSalaryRequestsCountAsync() => this.dbContext.SalaryRequests.CountAsync();

    public async Task<ServiceResult> UpdateAsync(int id, decimal newSalary, string? notes)
    {
        var updatedCount = await this.dbContext.SalaryRequests
            .Where(e => e.Id == id)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(e => e.NewSalary, newSalary)
                .SetProperty(e => e.Notes, notes));

        return updatedCount > 0 ? ServiceResult.Success : ServiceResult.ErrorNotFound;
    }
}
