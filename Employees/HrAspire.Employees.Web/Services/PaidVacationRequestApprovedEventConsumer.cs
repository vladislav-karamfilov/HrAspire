namespace HrAspire.Employees.Web.Services;

using System.Threading.Tasks;

using HrAspire.Business.Common.Events;
using HrAspire.Employees.Business.Employees;

using MassTransit;

public class PaidVacationRequestApprovedEventConsumer : IConsumer<PaidVacationRequestApprovedEvent>
{
    private readonly IEmployeesService employeesService;
    private readonly ILogger<PaidVacationRequestApprovedEventConsumer> logger;

    public PaidVacationRequestApprovedEventConsumer(
        IEmployeesService employeesService,
        ILogger<PaidVacationRequestApprovedEventConsumer> logger)
    {
        this.employeesService = employeesService;
        this.logger = logger;
    }

    public async Task Consume(ConsumeContext<PaidVacationRequestApprovedEvent> context)
    {
        var result = await this.employeesService.UpdateUsedPaidVacationDaysAsync(
            context.Message.EmployeeId,
            context.Message.TotalUsedPaidVacationDays);

        if (result.IsError)
        {
            this.logger.LogError(
                "Error consuming event for employee {EmployeeId}, from date {FromDate}, to date {ToDate}, total used paid vacation days {TotalUsedPaidVacationDays} and approved on {ApprovedOn}: {ErrorMessage}",
                context.Message.EmployeeId,
                context.Message.FromDate,
                context.Message.ToDate,
                context.Message.TotalUsedPaidVacationDays,
                context.Message.ApprovedOn,
                result.ErrorMessage);
        }
    }
}
