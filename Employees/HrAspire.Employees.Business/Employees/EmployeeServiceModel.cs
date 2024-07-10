namespace HrAspire.Employees.Business.Employees;

public record EmployeeServiceModel(
    string FullName,
    DateOnly DateOfBirth,
    string? Department,
    string Position,
    string? ManagerId,
    string? ManagerFullName,
    DateTime CreatedOn);
