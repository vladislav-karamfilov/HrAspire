namespace HrAspire.Web.Common.Models.Employees;
public class EmployeeCreateRequestModel
{
    public string Email { get; set; } = default!;

    public string Password { get; set; } = default!;

    public string ConfirmPassword { get; set; } = default!;

    public string FullName { get; set; } = default!;

    public DateOnly? DateOfBirth { get; set; }

    public decimal Salary { get; set; }

    public string Position { get; set; } = default!;

    public string? Department { get; set; }

    public string? ManagerId { get; set; }

    public string? Role { get; set; }
}
