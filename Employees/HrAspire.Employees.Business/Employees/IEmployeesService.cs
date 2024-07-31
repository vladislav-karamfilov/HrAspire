namespace HrAspire.Employees.Business.Employees;

using HrAspire.Business.Common;

public interface IEmployeesService
{
    Task<IEnumerable<EmployeeServiceModel>> GetEmployeesAsync(int pageNumber, int pageSize);

    Task<int> GetEmployeesCountAsync();

    Task<EmployeeDetailsServiceModel?> GetEmployeeAsync(string id);

    Task<ServiceResult<string>> CreateAsync(
        string email,
        string password,
        string fullName,
        DateOnly dateOfBirth,
        string position,
        string? department,
        string? managerId,
        string createdById);

    Task<ServiceResult> UpdateAsync(
        string id,
        string fullName,
        DateOnly dateOfBirth,
        string position,
        string? department,
        string? managerId);

    Task<ServiceResult> DeleteAsync(string id);
}
