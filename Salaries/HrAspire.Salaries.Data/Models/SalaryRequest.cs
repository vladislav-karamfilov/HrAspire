namespace HrAspire.Salaries.Data.Models;

using System;

using HrAspire.Data.Common.Models;

public class SalaryRequest : IDeletableEntity
{
    public int Id { get; set; }

    public string EmployeeId { get; set; } = default!;

    public decimal NewSalary { get; set; }

    public string? Notes { get; set; }

    public SalaryRequestStatus Status { get; set; }

    public string? StatusChangedById { get; set; }

    public DateTime? StatusChangedOn { get; set; }

    public string CreatedById { get; set; } = default!;

    public DateTime CreatedOn { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedOn { get; set; }
}
