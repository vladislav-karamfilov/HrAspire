namespace HrAspire.Vacations.Web.Services;

using System.Threading.Tasks;

using HrAspire.Business.Common.Events;
using HrAspire.Vacations.Business.VacationRequests;

using MassTransit;

public class EmployeeDeletedEventConsumer : IConsumer<EmployeeDeletedEvent>
{
    private readonly IVacationRequestsService vacationRequestsService;

    public EmployeeDeletedEventConsumer(IVacationRequestsService vacationRequestsService)
    {
        this.vacationRequestsService = vacationRequestsService;
    }

    public Task Consume(ConsumeContext<EmployeeDeletedEvent> context)
        => this.vacationRequestsService.DeleteEmployeeVacationRequestsAsync(context.Message.EmployeeId);
}
