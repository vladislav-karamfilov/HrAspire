namespace HrAspire.Employees.Business.Employees;

public class EmployeeDetailsServiceModel
{
    public string FullName { get; init; } = default!;

    public DateOnly DateOfBirth { get; init; }

    public string? Department { get; init; }

    public string Position { get; init; } = default!;

    public string? ManagerId { get; init; }

    public string? ManagerFullName { get; init; }

    public DateTime CreatedOn { get; init; }

    public string? CreatedByFullName { get; init; }
}
