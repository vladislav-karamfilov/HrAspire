namespace HrAspire.Employees.Business.Employees;

using HrAspire.Business.Common;

public interface IEmployeesService
{
    Task<IEnumerable<EmployeeServiceModel>> ListAsync(string currentEmployeeId, int pageNumber, int pageSize);

    Task<IEnumerable<EmployeeServiceModel>> ListManagersAsync();

    Task<int> GetCountAsync(string currentEmployeeId);

    Task<EmployeeDetailsServiceModel?> GetAsync(string id, string currentEmployeeId);

    Task<ServiceResult<string>> CreateAsync(
        string email,
        string password,
        string fullName,
        DateOnly dateOfBirth,
        string position,
        string? department,
        string? managerId,
        decimal salary,
        string role,
        string createdById);

    Task<ServiceResult> UpdateAsync(
        string id,
        string fullName,
        DateOnly dateOfBirth,
        string position,
        string? department,
        string? managerId,
        string role);

    Task<ServiceResult> DeleteAsync(string id);

    Task<ServiceResult> UpdateSalaryAsync(string id, decimal newSalary);

    Task<ServiceResult> UpdateUsedPaidVacationDaysAsync(string id, int usedPaidVacationDays);
}
