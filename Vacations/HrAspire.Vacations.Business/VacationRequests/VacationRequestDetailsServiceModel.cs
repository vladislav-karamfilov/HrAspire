namespace HrAspire.Vacations.Business.VacationRequests;

using HrAspire.Vacations.Data.Models;

public class VacationRequestDetailsServiceModel
{
    public int Id { get; set; }

    public string EmployeeId { get; set; } = default!;

    public string EmployeeFullName { get; set; } = default!;

    public VacationRequestType Type { get; set; }

    public DateOnly FromDate { get; set; }

    public DateOnly ToDate { get; set; }

    public string? Notes { get; set; }

    public VacationRequestStatus Status { get; set; }

    public DateTime? StatusChangedOn { get; set; }

    public string? StatusChangedById { get; set; }

    public string? StatusChangedByFullName { get; set; }

    public DateTime CreatedOn { get; set; }
}
