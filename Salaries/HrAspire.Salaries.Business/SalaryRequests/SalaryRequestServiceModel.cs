namespace HrAspire.Salaries.Business.SalaryRequests;

using HrAspire.Salaries.Data.Models;

public record SalaryRequestServiceModel(
    int Id,
    string EmployeeId,
    string EmployeeFullName,
    decimal NewSalary,
    SalaryRequestStatus Status,
    DateTime CreatedOn);
