namespace HrAspire.Employees.Business.Employees;

public record EmployeeDetailsServiceModel(
    string Id,
    string Email,
    string FullName,
    DateOnly DateOfBirth,
    string? Department,
    string Position,
    string? ManagerId,
    string? ManagerFullName,
    DateTime CreatedOn,
    string? CreatedById,
    string? CreatedByFullName);
