namespace HrAspire.Web.Common.Models.Employees;

public record EmployeesResponseModel(ICollection<EmployeeResponseModel> Employees, int Total);
