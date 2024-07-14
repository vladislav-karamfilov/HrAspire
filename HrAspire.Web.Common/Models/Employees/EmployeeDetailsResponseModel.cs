namespace HrAspire.Web.Common.Models.Employees;

using System;

public record EmployeeDetailsResponseModel(
    string Id,
    string Email,
    string FullName,
    DateOnly DateOfBirth,
    string? Department,
    string Position,
    string? ManagerId,
    string? ManagerFullName,
    DateTime CreatedOn,
    string? CreatedByFullName);
