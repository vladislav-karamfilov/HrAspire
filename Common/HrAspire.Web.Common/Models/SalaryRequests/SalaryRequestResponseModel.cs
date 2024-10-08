namespace HrAspire.Web.Common.Models.SalaryRequests;

using HrAspire.Salaries.Data.Models;

public record SalaryRequestResponseModel(
    int Id,
    string EmployeeId,
    string EmployeeFullName,
    decimal NewSalary,
    SalaryRequestStatus Status,
    DateTime CreatedOn);
