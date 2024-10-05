namespace HrAspire.Salaries.Business.SalaryRequests;

using HrAspire.Business.Common;

public interface ISalaryRequestsService
{
    Task<IEnumerable<SalaryRequestServiceModel>> ListAsync(int pageNumber, int pageSize);

    Task<int> GetCountAsync();

    Task<IEnumerable<SalaryRequestServiceModel>> ListEmployeeSalaryRequestsAsync(
        string employeeId,
        int pageNumber,
        int pageSize,
        string? managerId);

    Task<int> GetEmployeeSalaryRequestsCountAsync(string employeeId, string? managerId);

    Task<SalaryRequestDetailsServiceModel?> GetAsync(int id, string? managerId);

    Task<ServiceResult<int>> CreateAsync(string employeeId, decimal newSalary, string? notes, string createdById);

    Task<ServiceResult> UpdateAsync(int id, decimal newSalary, string? notes, string currentEmployeeId);

    Task<ServiceResult> DeleteAsync(int id, string currentEmployeeId);

    Task DeleteEmployeeSalaryRequestsAsync(string employeeId);

    Task<ServiceResult> ApproveAsync(int id, string approvedById);

    Task<ServiceResult> RejectAsync(int id, string rejectedById);
}
