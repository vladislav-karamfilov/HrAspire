namespace HrAspire.Salaries.Business.SalaryRequests;

using HrAspire.Salaries.Data.Models;

public class SalaryRequestDetailsServiceModel
{
    public int Id { get; set; }

    public string EmployeeId { get; set; } = default!;

    public string EmployeeFullName { get; set; } = default!;

    public decimal NewSalary { get; set; }

    public string? Notes { get; set; }

    public SalaryRequestStatus Status { get; set; }

    public DateTime? StatusChangedOn { get; set; }

    public string? StatusChangedById { get; set; }

    public string? StatusChangedByFullName { get; set; }

    public DateTime CreatedOn { get; set; }

    public string CreatedById { get; set; } = default!;

    public string CreatedByFullName { get; set; } = default!;
}
