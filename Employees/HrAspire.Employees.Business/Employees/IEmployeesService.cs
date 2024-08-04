namespace HrAspire.Employees.Business.Employees;

using HrAspire.Business.Common;

public interface IEmployeesService
{
    Task<IEnumerable<EmployeeServiceModel>> GetEmployeesAsync(string currentEmployeeId, int pageNumber, int pageSize);

    Task<IEnumerable<EmployeeServiceModel>> GetManagersAsync();

    Task<int> GetEmployeesCountAsync(string currentEmployeeId);

    Task<EmployeeDetailsServiceModel?> GetEmployeeAsync(string id);

    Task<ServiceResult<string>> CreateAsync(
        string email,
        string password,
        string fullName,
        DateOnly dateOfBirth,
        string position,
        string? department,
        string? managerId,
        string role,
        string createdById);

    Task<ServiceResult> UpdateAsync(
        string id,
        string fullName,
        DateOnly dateOfBirth,
        string position,
        string? department,
        string role,
        string? managerId);

    Task<ServiceResult> DeleteAsync(string id);
}
