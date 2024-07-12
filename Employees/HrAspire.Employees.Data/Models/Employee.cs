namespace HrAspire.Employees.Data.Models;

using Microsoft.AspNetCore.Identity;

public class Employee : IdentityUser
{
    // TODO: non-nullable
    public string? FullName { get; set; } = default!;

    public DateOnly DateOfBirth { get; set; }

    public string? Department { get; set; }

    // TODO: non-nullable
    public string? Position { get; set; } = default!;

    public string? ManagerId { get; set; }

    public Employee? Manager { get; set; }

    public DateTime CreatedOn { get; set; }

    public string? CreatedById { get; set; }

    public Employee? CreatedBy { get; set; }

    public ICollection<Document> Documents { get; set; } = [];

    public ICollection<Document> CreatedDocuments { get; set; } = [];

    public ICollection<Employee> CreatedEmployees { get; set; } = [];
}
