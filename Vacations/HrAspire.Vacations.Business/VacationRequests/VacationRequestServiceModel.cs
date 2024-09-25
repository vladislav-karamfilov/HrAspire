namespace HrAspire.Vacations.Business.VacationRequests;

using HrAspire.Vacations.Data.Models;

public class VacationRequestServiceModel
{
    public int Id { get; set; }

    public string EmployeeId { get; set; } = default!;

    public string EmployeeFullName { get; set; } = default!;

    public VacationRequestType Type { get; set; }

    public DateOnly FromDate { get; set; }

    public DateOnly ToDate { get; set; }

    public VacationRequestStatus Status { get; set; }

    public DateTime CreatedOn { get; set; }
}
