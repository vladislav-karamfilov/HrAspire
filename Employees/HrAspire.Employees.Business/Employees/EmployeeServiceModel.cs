namespace HrAspire.Employees.Business.Employees;

public record EmployeeServiceModel(string Id, string Email, string FullName, string? Department, string Position, DateTime CreatedOn);
