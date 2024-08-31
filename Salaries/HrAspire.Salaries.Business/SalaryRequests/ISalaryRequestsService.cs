namespace HrAspire.Salaries.Business.SalaryRequests;

using HrAspire.Business.Common;

public interface ISalaryRequestsService
{
    Task<IEnumerable<SalaryRequestServiceModel>> GetSalaryRequestsAsync(int pageNumber, int pageSize);

    Task<int> GetSalaryRequestsCountAsync();

    Task<SalaryRequestDetailsServiceModel?> GetSalaryRequestAsync(int id);

    Task<ServiceResult<int>> CreateAsync(string employeeId, decimal newSalary, string? notes, string createdById);

    Task<ServiceResult> UpdateAsync(int id, decimal newSalary, string? notes);

    Task<ServiceResult> DeleteAsync(int id);
}
