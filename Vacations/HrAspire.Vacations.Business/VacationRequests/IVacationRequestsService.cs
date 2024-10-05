namespace HrAspire.Vacations.Business.VacationRequests;

using HrAspire.Business.Common;
using HrAspire.Vacations.Data.Models;

public interface IVacationRequestsService
{
    Task<IEnumerable<VacationRequestServiceModel>> ListEmployeeVacationRequestsAsync(
        string employeeId,
        int pageNumber,
        int pageSize,
        string currentEmployeeId);

    Task<int> GetEmployeeVacationRequestsCountAsync(string employeeId, string currentEmployeeId);

    Task<VacationRequestDetailsServiceModel?> GetAsync(int id, string currentEmployeeId);

    Task<ServiceResult<int>> CreateAsync(string employeeId, VacationRequestType type, DateOnly fromDate, DateOnly toDate, string? notes);

    Task<ServiceResult> UpdateAsync(
        int id,
        VacationRequestType type,
        DateOnly fromDate,
        DateOnly toDate,
        string? notes,
        string currentEmployeeId);

    Task<ServiceResult> DeleteAsync(int id, string currentEmployeeId);

    Task DeleteEmployeeVacationRequestsAsync(string employeeId);

    Task<ServiceResult> ApproveAsync(int id, string approvedById);

    Task<ServiceResult> RejectAsync(int id, string rejectedById);
}
