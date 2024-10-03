namespace HrAspire.Web.Common.Models.Employees;

using System;

public record EmployeeDetailsResponseModel(
    string Id,
    string Email,
    string FullName,
    string? Role,
    DateOnly DateOfBirth,
    string? Department,
    string Position,
    string? ManagerId,
    string? ManagerFullName,
    decimal Salary,
    int UsedPaidVacationDays,
    DateTime CreatedOn,
    string? CreatedById,
    string? CreatedByFullName);
