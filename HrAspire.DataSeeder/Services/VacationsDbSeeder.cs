namespace HrAspire.DataSeeder.Services;

using HrAspire.Vacations.Business.VacationRequests;
using HrAspire.Vacations.Data.Models;

public class VacationsDbSeeder
{
    private readonly IVacationRequestsService vacationRequestsService;
    private readonly ILogger<VacationsDbSeeder> logger;

    public VacationsDbSeeder(IVacationRequestsService vacationRequestsService, ILogger<VacationsDbSeeder> logger)
    {
        this.vacationRequestsService = vacationRequestsService;
        this.logger = logger;
    }

    public async Task SeedVacationRequestsAsync(IEnumerable<string> employeeIds)
    {
        foreach (var employeeId in employeeIds)
        {
            if (Random.Shared.Next() % 2 == 0)
            {
                continue;
            }

            var isPaid = Random.Shared.Next() % 2 == 1;
            var startOffsetDays = Random.Shared.Next(0, 31);
            var durationDays = Random.Shared.Next(3, 11);

            var startDate = DateOnly.FromDateTime(DateTime.Today.AddDays(startOffsetDays));

            var vacationRequestResult = await this.vacationRequestsService.CreateAsync(
                employeeId,
                isPaid ? VacationRequestType.Paid : VacationRequestType.Unpaid,
                startDate,
                startDate.AddDays(durationDays),
                notes: null);

            if (vacationRequestResult.IsError)
            {
                throw new Exception("Error creating vacation request: " + vacationRequestResult.ErrorMessage);
            }

            this.logger.LogInformation(
                "Created vacation request {VacationRequestId} for employee {EmployeeId}", vacationRequestResult.Data, employeeId);
        }
    }
}
