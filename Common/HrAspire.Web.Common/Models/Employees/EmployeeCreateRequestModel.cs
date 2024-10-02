namespace HrAspire.Web.Common.Models.Employees;

using System.ComponentModel.DataAnnotations;

public class EmployeeCreateRequestModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = default!;

    [Required]
    [MinLength(AccountConstants.PasswordMinLength, ErrorMessage = "{0} must be at least {1} characters long.")]
    public string Password { get; set; } = default!;

    [Compare(nameof(Password))]
    public string ConfirmPassword { get; set; } = default!;

    [Required]
    public string FullName { get; set; } = default!;

    [Required]
    public DateOnly? DateOfBirth { get; set; }

    [Range(0, int.MaxValue)]
    public decimal Salary { get; set; }

    [Required]
    public string Position { get; set; } = default!;

    public string? Department { get; set; }

    public string? ManagerId { get; set; }

    public string? Role { get; set; }
}
