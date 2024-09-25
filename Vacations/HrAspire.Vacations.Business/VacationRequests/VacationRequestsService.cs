namespace HrAspire.Vacations.Business.VacationRequests;

using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

using HrAspire.Business.Common;
using HrAspire.Business.Common.Events;
using HrAspire.Data.Common.Models;
using HrAspire.Vacations.Business.Mappers;
using HrAspire.Vacations.Data;
using HrAspire.Vacations.Data.Models;

using Microsoft.EntityFrameworkCore;

using StackExchange.Redis;

public class VacationRequestsService : IVacationRequestsService
{
    private readonly VacationsDbContext dbContext;
    private readonly IConnectionMultiplexer cacheConnectionMultiplexer;
    private readonly TimeProvider timeProvider;

    public VacationRequestsService(
        VacationsDbContext dbContext,
        IConnectionMultiplexer cacheConnectionMultiplexer,
        TimeProvider timeProvider)
    {
        this.dbContext = dbContext;
        this.cacheConnectionMultiplexer = cacheConnectionMultiplexer;
        this.timeProvider = timeProvider;
    }

    private IDatabase CacheDatabase => this.cacheConnectionMultiplexer.GetDatabase();

    public async Task<ServiceResult<int>> CreateAsync(
        string employeeId,
        VacationRequestType type,
        DateOnly fromDate,
        DateOnly toDate,
        string? notes)
    {
        if (fromDate > toDate)
        {
            return ServiceResult<int>.Error("Vacation From date cannot be after its To date.");
        }

        var workDays = VacationRequestHelper.CalculateWorkDaysBetweenDates(fromDate, toDate);
        if (workDays == 0)
        {
            return ServiceResult<int>.Error("No work days between selected vacation dates.");
        }

        if (!await this.EmployeeExistsAsync(employeeId))
        {
            return ServiceResult<int>.Error("Employee to create vacation request for doesn't exist.");
        }

        // TODO: Check if the employee has enough days left to have the paid vacation (take years into account)
        var vacationRequest = new VacationRequest
        {
            EmployeeId = employeeId,
            Type = type,
            FromDate = fromDate,
            ToDate = toDate,
            WorkDays = workDays,
            Notes = notes,
            Status = VacationRequestStatus.Pending,
            CreatedOn = this.timeProvider.GetUtcNow().UtcDateTime,
        };

        this.dbContext.Add(vacationRequest);
        await this.dbContext.SaveChangesAsync();

        return ServiceResult<int>.Success(vacationRequest.Id);
    }

    public async Task<ServiceResult> DeleteAsync(int id)
    {
        // TODO: Don't allow update of approved (same for salary reqs)
        var deletedCount = await this.dbContext.VacationRequests
            .Where(e => e.Id == id)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(e => e.IsDeleted, true)
                .SetProperty(e => e.DeletedOn, this.timeProvider.GetUtcNow().UtcDateTime));

        return deletedCount > 0 ? ServiceResult.Success : ServiceResult.ErrorNotFound;
    }

    public async Task<VacationRequestDetailsServiceModel?> GetAsync(int id)
    {
        var vacationRequest = await this.dbContext.VacationRequests
            .Where(r => r.Id == id)
            .ProjectToDetailsServiceModel()
            .FirstOrDefaultAsync();

        if (vacationRequest is null)
        {
            return null;
        }

        var employeeNames = await this.GetEmployeeNamesAsync(vacationRequest.EmployeeId, vacationRequest.StatusChangedById);

        vacationRequest.EmployeeFullName = employeeNames[0];
        vacationRequest.StatusChangedByFullName = employeeNames[1];

        return vacationRequest;
    }

    public async Task<IEnumerable<VacationRequestServiceModel>> ListEmployeeVacationRequestsAsync(
        string employeeId,
        int pageNumber,
        int pageSize)
    {
        PaginationHelper.Normalize(ref pageNumber, ref pageSize);

        var result = await this.dbContext.VacationRequests
            .Where(e => e.EmployeeId == employeeId)
            .OrderByDescending(d => d.CreatedOn)
            .Skip(pageNumber * pageSize)
            .Take(pageSize)
            .ProjectToServiceModel()
            .ToListAsync();

        await this.PopulateEmployeeNamesAsync(result);

        return result;
    }

    public Task<int> GetEmployeeVacationRequestsCountAsync(string employeeId)
        => this.dbContext.VacationRequests.CountAsync(r => r.EmployeeId == employeeId);

    public async Task<ServiceResult> UpdateAsync(int id, VacationRequestType type, DateOnly fromDate, DateOnly toDate, string? notes)
    {
        // TODO: don't allow update of non-pending (same for salary reqs)
        var updatedCount = await this.dbContext.VacationRequests
            .Where(e => e.Id == id)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(e => e.Type, type)
                .SetProperty(e => e.FromDate, fromDate)
                .SetProperty(e => e.ToDate, toDate)
                .SetProperty(e => e.Notes, notes));

        return updatedCount > 0 ? ServiceResult.Success : ServiceResult.ErrorNotFound;
    }

    public async Task<ServiceResult> ApproveAsync(int id, string approvedById)
    {
        if (!await this.EmployeeExistsAsync(approvedById))
        {
            return ServiceResult<int>.Error("Vacation request approving employee doesn't exist.");
        }

        var vacationRequest = await this.dbContext.VacationRequests.FirstOrDefaultAsync(r => r.Id == id);
        if (vacationRequest is null)
        {
            return ServiceResult.ErrorNotFound;
        }

        if (vacationRequest.Status == VacationRequestStatus.Approved)
        {
            return ServiceResult.Success;
        }

        if (vacationRequest.Status == VacationRequestStatus.Rejected)
        {
            return ServiceResult.Error("Vacation request has already been rejected.");
        }

        if (vacationRequest.Type == VacationRequestType.Paid)
        {
            // TODO: Check if the employee has enough days left to have the paid vacation
            // TODO:
            //this.dbContext.OutboxMessages.Add(new OutboxMessage
            //{
            //    Type = typeof(SalaryRequestApprovedEvent).FullName!,
            //    Payload = JsonSerializer.Serialize(new SalaryRequestApprovedEvent(vacationRequest.EmployeeId, vacationRequest.NewSalary, utcNow)),
            //    CreatedOn = utcNow,
            //});
        }

        var utcNow = this.timeProvider.GetUtcNow().UtcDateTime;

        vacationRequest.Status = VacationRequestStatus.Approved;
        vacationRequest.StatusChangedOn = utcNow;
        vacationRequest.StatusChangedById = approvedById;

        await this.dbContext.SaveChangesAsync();

        return ServiceResult.Success;
    }

    public async Task<ServiceResult> RejectAsync(int id, string rejectedById)
    {
        if (!await this.EmployeeExistsAsync(rejectedById))
        {
            return ServiceResult<int>.Error("Vacation request rejecting employee doesn't exist.");
        }

        var vacationRequest = await this.dbContext.VacationRequests.FirstOrDefaultAsync(r => r.Id == id);
        if (vacationRequest is null)
        {
            return ServiceResult.ErrorNotFound;
        }

        if (vacationRequest.Status == VacationRequestStatus.Rejected)
        {
            return ServiceResult.Success;
        }

        if (vacationRequest.Status == VacationRequestStatus.Approved)
        {
            return ServiceResult.Error("Vacation request has already been approved.");
        }

        vacationRequest.Status = VacationRequestStatus.Rejected;
        vacationRequest.StatusChangedOn = this.timeProvider.GetUtcNow().UtcDateTime;
        vacationRequest.StatusChangedById = rejectedById;

        await this.dbContext.SaveChangesAsync();

        return ServiceResult.Success;
    }

    private Task<bool> EmployeeExistsAsync(string employeeId)
        => this.CacheDatabase.HashExistsAsync(BusinessConstants.EmployeeNamesCacheSetName, employeeId);

    private async Task<string[]> GetEmployeeNamesAsync(params string?[] employeeIds)
    {
        var employeeNames = await this.CacheDatabase.HashGetAsync(
            BusinessConstants.EmployeeNamesCacheSetName,
            employeeIds.Select(e => (RedisValue)(e ?? string.Empty)).ToArray());

        return employeeNames.Select(n => n == RedisValue.Null ? string.Empty : n.ToString()).ToArray();
    }

    private async Task PopulateEmployeeNamesAsync(List<VacationRequestServiceModel> vacationRequests)
    {
        var cachedNames = new Dictionary<string, string>();
        foreach (var vacationRequest in vacationRequests)
        {
            if (!cachedNames.TryGetValue(vacationRequest.EmployeeId, out var employeeFullName))
            {
                var employeeNames = await this.GetEmployeeNamesAsync(vacationRequest.EmployeeId);
                employeeFullName = employeeNames[0];

                cachedNames[vacationRequest.EmployeeId] = employeeFullName;
            }

            vacationRequest.EmployeeFullName = employeeFullName;
        }
    }
}
