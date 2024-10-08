namespace HrAspire.Employees.Web.Services;

using System.Threading.Tasks;

using HrAspire.Business.Common.Events;
using HrAspire.Employees.Business.Employees;

using MassTransit;

public class SalaryRequestApprovedEventConsumer : IConsumer<SalaryRequestApprovedEvent>
{
    private readonly IEmployeesService employeesService;
    private readonly ILogger<SalaryRequestApprovedEventConsumer> logger;

    public SalaryRequestApprovedEventConsumer(IEmployeesService employeesService, ILogger<SalaryRequestApprovedEventConsumer> logger)
    {
        this.employeesService = employeesService;
        this.logger = logger;
    }

    public async Task Consume(ConsumeContext<SalaryRequestApprovedEvent> context)
    {
        var result = await this.employeesService.UpdateSalaryAsync(context.Message.EmployeeId, context.Message.NewSalary);
        if (result.IsError)
        {
            this.logger.LogError(
                "Error consuming event for employee {EmployeeId}, salary {Salary} and approved on {ApprovedOn}: {ErrorMessage}",
                context.Message.EmployeeId,
                context.Message.NewSalary,
                context.Message.ApprovedOn,
                result.ErrorMessage);
        }
    }
}
