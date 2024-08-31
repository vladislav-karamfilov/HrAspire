namespace HrAspire.Salaries.Business.SalaryRequests;

using HrAspire.Salaries.Data.Models;

public class SalaryRequestServiceModel
{
    public int Id { get; set; }

    public string EmployeeId { get; set; } = default!;

    public string EmployeeFullName { get; set; } = default!;

    public decimal NewSalary { get; set; }

    public SalaryRequestStatus Status { get; set; }

    public DateTime CreatedOn { get; set; }
}
