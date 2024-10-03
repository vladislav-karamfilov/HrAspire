namespace HrAspire.Employees.Business.Employees;

public class EmployeeDetailsServiceModel
{
    public string Id { get; set; } = default!;

    public string Email { get; set; } = default!;

    public string FullName { get; set; } = default!;

    public string? Role { get; set; }

    public DateOnly DateOfBirth { get; set; }

    public string? Department { get; set; }

    public string Position { get; set; } = default!;

    public string? ManagerId { get; set; }

    public string? ManagerFullName { get; set; }

    public decimal Salary { get; set; }

    public int UsedPaidVacationDays { get; set; }

    public DateTime CreatedOn { get; set; }

    public string? CreatedById { get; set; }

    public string? CreatedByFullName { get; set; }
}
