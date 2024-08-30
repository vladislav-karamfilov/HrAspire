namespace HrAspire.Salaries.Business.SalaryRequests;

using HrAspire.Salaries.Data.Models;

public record SalaryRequestDetailsServiceModel(
    int Id,
    string EmployeeId,
    string EmployeeFullName,
    decimal NewSalary,
    string? Notes,
    SalaryRequestStatus Status,
    DateTime? StatusChangedOn,
    string? StatusChangedById,
    string? StatusChangedByFullName,
    DateTime CreatedOn,
    string CreatedById,
    string CreatedByFullName);
