namespace HrAspire.Employees.Business.Employees;

public record EmployeeDetailsServiceModel(
    string Id,
    string FullName,
    DateOnly DateOfBirth,
    string? Department,
    string Position,
    string? ManagerId,
    string? ManagerFullName,
    DateTime CreatedOn,
    string? CreatedByFullName);
