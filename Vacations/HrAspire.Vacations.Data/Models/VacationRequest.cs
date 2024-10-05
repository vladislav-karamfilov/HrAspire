namespace HrAspire.Vacations.Data.Models;

using System;

using HrAspire.Data.Common.Models;

public class VacationRequest : IDeletableEntity
{
    public int Id { get; set; }

    public string EmployeeId { get; set; } = default!;

    public VacationRequestType Type { get; set; }

    public DateOnly FromDate { get; set; }

    public DateOnly ToDate { get; set; }

    public int WorkDays { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedOn { get; set; }

    public VacationRequestStatus Status { get; set; }

    public string? StatusChangedById { get; set; }

    public DateTime? StatusChangedOn { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedOn { get; set; }
}
