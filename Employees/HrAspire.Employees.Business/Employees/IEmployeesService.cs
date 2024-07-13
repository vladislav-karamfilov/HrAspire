namespace HrAspire.Employees.Business.Employees;

public interface IEmployeesService
{
    Task<IEnumerable<EmployeeServiceModel>> GetEmployeesPageAsync(int pageNumber, int pageSize);

    Task<int> GetEmployeesCountAsync();

    Task<EmployeeDetailsServiceModel?> GetEmployeeAsync(string id);
}
