namespace HrAspire.Web.Common.Models.Employees;

public record EmployeesPageResponseModel(ICollection<EmployeeResponseModel> Employees, int Total);
