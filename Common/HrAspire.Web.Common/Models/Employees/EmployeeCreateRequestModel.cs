namespace HrAspire.Web.Common.Models.Employees;

using System.ComponentModel.DataAnnotations;

public class EmployeeCreateRequestModel
{
    [Required]
    public string Email { get; set; } = default!;

    [Required]
    public string Password { get; set; } = default!;

    [Compare(nameof(Password))]
    public string ConfirmPassword { get; set; } = default!;

    [Required]
    public string FullName { get; set; } = default!;

    [Required]
    public DateOnly? DateOfBirth { get; set; }

    [Required]
    public string Position { get; set; } = default!;

    public string? Department { get; set; }

    public string? ManagerId { get; set; }
}
