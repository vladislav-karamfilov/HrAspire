namespace HrAspire.Employees.Business.Employees;

public interface IEmployeesService
{
    Task<IEnumerable<EmployeeServiceModel>> GetEmployeesPageAsync(int pageNumber, int pageSize);

    Task<EmployeeDetailsServiceModel?> GetEmployeeAsync(string id);
}
