namespace HrAspire.Employees.Data.Models;

public class Document
{
    public int Id { get; set; }

    public required string Title { get; set; }

    public required string Url { get; set; }

    public string EmployeeId { get; set; } = default!;

    public Employee Employee { get; set; } = default!;

    public string CreatedById { get; set; } = default!;

    public Employee CreatedBy { get; set; } = default!;

    public DateTime CreatedOn { get; set; }
}
