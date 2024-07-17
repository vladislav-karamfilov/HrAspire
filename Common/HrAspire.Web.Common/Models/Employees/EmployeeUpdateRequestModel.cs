namespace HrAspire.Web.Common.Models.Employees;

using System.ComponentModel.DataAnnotations;

public class EmployeeUpdateRequestModel
{
    [Required]
    public string FullName { get; set; } = default!;

    public DateOnly DateOfBirth { get; set; }

    [Required]
    public string Position { get; set; } = default!;

    public string? Department { get; set; }

    public string? ManagerId { get; set; }
}
