namespace HrAspire.Web.Common.Models.SalaryRequests;

using HrAspire.Salaries.Data.Models;

public record SalaryRequestDetailsResponseModel(
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
