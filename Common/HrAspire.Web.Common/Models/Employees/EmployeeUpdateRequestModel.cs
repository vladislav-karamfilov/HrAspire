namespace HrAspire.Web.Common.Models.Employees;

public class EmployeeUpdateRequestModel
{
    public string FullName { get; set; } = default!;

    public DateOnly DateOfBirth { get; set; }

    public string Position { get; set; } = default!;

    public string? Department { get; set; }

    public string? ManagerId { get; set; }

    public string? Role { get; set; }
}
