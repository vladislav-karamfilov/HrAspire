namespace HrAspire.Employees.Data.Models;

using Microsoft.AspNetCore.Identity;

public class Employee : IdentityUser
{
    public override required string? UserName { get; set; }

    public override required string? Email { get; set; }

    public required string FullName { get; set; }

    public DateOnly DateOfBirth { get; set; }

    public string? Department { get; set; }

    public required string Position { get; set; }

    public string? ManagerId { get; set; }

    public Employee? Manager { get; set; }

    public DateTime CreatedOn { get; set; }

    public ICollection<Document> Documents { get; set; } = [];

    public ICollection<Document> CreatedDocuments { get; set; } = [];
}
