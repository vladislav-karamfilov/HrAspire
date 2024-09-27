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
        if (!await this.EmployeeExistsAsync(employeeId))
        {
            return ServiceResult<int>.Error("Employee to create vacation request for doesn't exist.");
        }

        if (fromDate > toDate)
        {
            return ServiceResult<int>.Error("Vacation From date cannot be after its To date.");
        }

        var workDays = VacationRequestHelper.CalculateWorkDaysBetweenDates(fromDate, toDate);
        if (workDays <= 0)
        {
            return ServiceResult<int>.Error("No work days between selected vacation dates.");
        }

        if (type == VacationRequestType.Paid)
        {
            var (hasEnoughPaidVacationDays, _) = await this.CheckIfEmployeeCanUsePaidVacationAsync(
                employeeId,
                fromDate,
                toDate,
                workDays);

            if (!hasEnoughPaidVacationDays)
            {
                return ServiceResult<int>.Error(
                    $"Not enough paid vacation days left for this vacation. Max paid vacation days per year: {BusinessConstants.MaxPaidVacationDaysPerYear}");
            }
        }

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
        var vacationRequest = await this.dbContext.VacationRequests.FirstOrDefaultAsync(r => r.Id == id);
        if (vacationRequest is null)
        {
            return ServiceResult.ErrorNotFound;
        }

        if (vacationRequest.Status == VacationRequestStatus.Approved)
        {
            return ServiceResult.Error("Vacation request is approved and cannot be deleted.");
        }

        vacationRequest.IsDeleted = true;
        vacationRequest.DeletedOn = this.timeProvider.GetUtcNow().UtcDateTime;

        await this.dbContext.SaveChangesAsync();

        return ServiceResult.Success;
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
        var vacationRequest = await this.dbContext.VacationRequests.FirstOrDefaultAsync(r => r.Id == id);
        if (vacationRequest is null)
        {
            return ServiceResult.ErrorNotFound;
        }

        if (vacationRequest.Status != VacationRequestStatus.Pending)
        {
            return ServiceResult.Error("Vacation request is not pending and cannot be updated.");
        }

        if (fromDate > toDate)
        {
            return ServiceResult<int>.Error("Vacation From date cannot be after its To date.");
        }

        var workDays = VacationRequestHelper.CalculateWorkDaysBetweenDates(fromDate, toDate);
        if (workDays <= 0)
        {
            return ServiceResult<int>.Error("No work days between selected vacation dates.");
        }

        if (type == VacationRequestType.Paid)
        {
            var (hasEnoughPaidVacationDays, _) = await this.CheckIfEmployeeCanUsePaidVacationAsync(
                vacationRequest.EmployeeId,
                fromDate,
                toDate,
                workDays,
                vacationRequest.Id);

            if (!hasEnoughPaidVacationDays)
            {
                return ServiceResult<int>.Error(
                    $"Not enough paid vacation days left for this vacation. Max paid vacation days per year: {BusinessConstants.MaxPaidVacationDaysPerYear}");
            }
        }

        vacationRequest.Type = type;
        vacationRequest.FromDate = fromDate;
        vacationRequest.ToDate = toDate;
        vacationRequest.WorkDays = workDays;
        vacationRequest.Notes = notes;

        await this.dbContext.SaveChangesAsync();

        return ServiceResult.Success;
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

        var utcNow = this.timeProvider.GetUtcNow().UtcDateTime;

        if (vacationRequest.Type == VacationRequestType.Paid)
        {
            var (hasEnoughPaidVacationDays, totalUsedPaidVacationDays) = await this.CheckIfEmployeeCanUsePaidVacationAsync(
                vacationRequest.EmployeeId,
                vacationRequest.FromDate,
                vacationRequest.ToDate,
                vacationRequest.WorkDays,
                vacationRequest.Id);

            if (!hasEnoughPaidVacationDays)
            {
                return ServiceResult<int>.Error(
                    $"Not enough paid vacation days left for this vacation. Max paid vacation days per year: {BusinessConstants.MaxPaidVacationDaysPerYear}");
            }

            this.dbContext.OutboxMessages.Add(new OutboxMessage
            {
                Type = typeof(PaidVacationRequestApprovedEvent).FullName!,
                CreatedOn = utcNow,
                Payload = JsonSerializer.Serialize(
                    new PaidVacationRequestApprovedEvent(
                        vacationRequest.EmployeeId,
                        vacationRequest.FromDate,
                        vacationRequest.ToDate,
                        totalUsedPaidVacationDays,
                        utcNow)),
            });
        }

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

    private async Task<(bool HasEnoughPaidVacationDays, int TotalUsedPaidVacationDays)> CheckIfEmployeeCanUsePaidVacationAsync(
        string employeeId,
        DateOnly fromDate,
        DateOnly toDate,
        int workDays,
        int? currentVacationRequestId = null)
    {
        var fromYear = fromDate.Year;
        var toYear = toDate.Year;

        if (fromYear == toYear)
        {
            var approvedVacationRequestsQuery = this.dbContext.VacationRequests
                .Where(r => r.EmployeeId == employeeId && r.Status == VacationRequestStatus.Approved && r.FromDate.Year == fromYear);

            if (currentVacationRequestId.HasValue)
            {
                approvedVacationRequestsQuery = approvedVacationRequestsQuery.Where(r => r.Id != currentVacationRequestId.Value);
            }

            var approvedVacationRequests = await approvedVacationRequestsQuery.Select(r => new { r.FromDate, r.ToDate }).ToListAsync();

            var usedWorkDays = 0;
            foreach (var request in approvedVacationRequests)
            {
                var currentToDate = request.ToDate.Year == fromYear
                    ? request.ToDate
                    : new DateOnly(request.FromDate.Year, 12, 31);

                usedWorkDays += VacationRequestHelper.CalculateWorkDaysBetweenDates(request.FromDate, currentToDate);
            }

            var totalUsedPaidVacationDays = usedWorkDays + workDays;

            return (totalUsedPaidVacationDays <= BusinessConstants.MaxPaidVacationDaysPerYear, totalUsedPaidVacationDays);
        }
        else if (toYear - fromYear > 1)
        {
            // Vacation days > 1 year -> always invalid
            return (false, 0);
        }
        else
        {
            var approvedVacationRequestsQuery = this.dbContext.VacationRequests.Where(r =>
                r.EmployeeId == employeeId &&
                r.Status == VacationRequestStatus.Approved &&
                (r.FromDate.Year == fromYear || r.ToDate.Year == toYear));

            if (currentVacationRequestId.HasValue)
            {
                approvedVacationRequestsQuery = approvedVacationRequestsQuery.Where(r => r.Id != currentVacationRequestId.Value);
            }

            var approvedVacationRequests = await approvedVacationRequestsQuery.Select(r => new { r.FromDate, r.ToDate }).ToListAsync();

            var endOfFromYearDate = new DateOnly(fromYear, 12, 31);
            var startOfToYearDate = new DateOnly(toYear, 1, 1);

            var usedWorkDaysForFromYear = 0;
            var usedWorkDaysForToYear = 0;
            foreach (var request in approvedVacationRequests)
            {
                if (request.FromDate.Year == fromYear)
                {
                    var currentToDate = request.ToDate.Year == fromYear
                        ? request.ToDate
                        : endOfFromYearDate;

                    usedWorkDaysForFromYear += VacationRequestHelper.CalculateWorkDaysBetweenDates(request.FromDate, currentToDate);
                }
                else
                {
                    var currentFromDate = request.FromDate.Year == toYear
                        ? request.FromDate
                        : startOfToYearDate;

                    usedWorkDaysForToYear += VacationRequestHelper.CalculateWorkDaysBetweenDates(currentFromDate, request.ToDate);
                }
            }

            var workDaysForFromYear = VacationRequestHelper.CalculateWorkDaysBetweenDates(fromDate, endOfFromYearDate);
            var workDaysForToYear = VacationRequestHelper.CalculateWorkDaysBetweenDates(startOfToYearDate, toDate);

            var totalUsedPaidVacationDaysForFromYear = usedWorkDaysForFromYear + workDaysForFromYear;
            var totalUsedPaidVacationDaysForToYear = usedWorkDaysForToYear + workDaysForToYear;

            var hasEnoughPaidVacationDays =
                totalUsedPaidVacationDaysForFromYear <= BusinessConstants.MaxPaidVacationDaysPerYear &&
                totalUsedPaidVacationDaysForToYear <= BusinessConstants.MaxPaidVacationDaysPerYear;

            return (hasEnoughPaidVacationDays, totalUsedPaidVacationDaysForFromYear);
        }
    }
}
