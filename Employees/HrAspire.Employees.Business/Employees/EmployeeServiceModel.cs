namespace HrAspire.Employees.Business.Employees;

public record EmployeeServiceModel(
    string Id,
    string Email,
    string FullName,
    DateOnly DateOfBirth,
    string? Department,
    string Position,
    DateTime CreatedOn);
