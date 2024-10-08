namespace HrAspire.Salaries.Web.Services;

using System.Threading.Tasks;

using HrAspire.Business.Common.Events;
using HrAspire.Salaries.Business.SalaryRequests;

using MassTransit;

public class EmployeeDeletedEventConsumer : IConsumer<EmployeeDeletedEvent>
{
    private readonly ISalaryRequestsService salaryRequestsService;

    public EmployeeDeletedEventConsumer(ISalaryRequestsService salaryRequestsService)
    {
        this.salaryRequestsService = salaryRequestsService;
    }

    public Task Consume(ConsumeContext<EmployeeDeletedEvent> context)
        => this.salaryRequestsService.DeleteEmployeeSalaryRequestsAsync(context.Message.EmployeeId);
}
