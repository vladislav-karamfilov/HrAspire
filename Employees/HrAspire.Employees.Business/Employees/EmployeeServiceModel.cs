namespace HrAspire.Employees.Business.Employees;

public class EmployeeServiceModel
{
    public string FullName { get; init; } = default!;

    public DateOnly DateOfBirth { get; init; }

    public string? Department { get; init; }

    public string Position { get; init; } = default!;

    public DateTime CreatedOn { get; init; }
}
