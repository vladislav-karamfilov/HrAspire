namespace HrAspire.Employees.Data.Models;

public class Document
{
    public int Id { get; set; }

    public string Title { get; set; } = default!;

    public string? Description { get; set; }

    public string Url { get; set; } = default!;

    public string FileName { get; set; } = default!;

    public string EmployeeId { get; set; } = default!;

    public Employee? Employee { get; set; }

    public string CreatedById { get; set; } = default!;

    public Employee? CreatedBy { get; set; }

    public DateTime CreatedOn { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedOn { get; set; }
}
