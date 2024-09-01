namespace HrAspire.Salaries.Business.SalaryRequests;

using HrAspire.Business.Common;

public interface ISalaryRequestsService
{
    Task<IEnumerable<SalaryRequestServiceModel>> ListAsync(int pageNumber, int pageSize);

    Task<int> GetCountAsync();

    Task<SalaryRequestDetailsServiceModel?> GetAsync(int id);

    Task<ServiceResult<int>> CreateAsync(string employeeId, decimal newSalary, string? notes, string createdById);

    Task<ServiceResult> UpdateAsync(int id, decimal newSalary, string? notes);

    Task<ServiceResult> DeleteAsync(int id);

    Task<ServiceResult> ApproveAsync(int id, string approvedById);

    Task<ServiceResult> RejectAsync(int id, string rejectedById);
}
