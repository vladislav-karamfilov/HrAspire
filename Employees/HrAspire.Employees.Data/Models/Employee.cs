namespace HrAspire.Employees.Data.Models;

using Microsoft.AspNetCore.Identity;

public class Employee : IdentityUser
{
    public required string FullName { get; set; }

    public DateOnly DateOfBirth { get; set; }

    public required string Department { get; set; }

    public required string Position { get; set; }

    public string? ManagerId { get; set; }

    public Employee? Manager { get; set; }

    public DateTime CreatedOn { get; set; }

    public ICollection<Document> Documents { get; set; } = [];

    public ICollection<Document> CreatedDocuments { get; set; } = [];
}
