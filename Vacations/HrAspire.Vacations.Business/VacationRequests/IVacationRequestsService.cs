namespace HrAspire.Vacations.Business.VacationRequests;

using HrAspire.Business.Common;
using HrAspire.Vacations.Data.Models;

public interface IVacationRequestsService
{
    Task<IEnumerable<VacationRequestServiceModel>> ListEmployeeVacationRequestsAsync(string employeeId, int pageNumber, int pageSize);

    Task<int> GetEmployeeVacationRequestsCountAsync(string employeeId);

    Task<VacationRequestDetailsServiceModel?> GetAsync(int id);

    Task<ServiceResult<int>> CreateAsync(string employeeId, VacationRequestType type, DateOnly fromDate, DateOnly toDate, string? notes);

    Task<ServiceResult> UpdateAsync(int id, VacationRequestType type, DateOnly fromDate, DateOnly toDate, string? notes);

    Task<ServiceResult> DeleteAsync(int id);

    Task<ServiceResult> ApproveAsync(int id, string approvedById);

    Task<ServiceResult> RejectAsync(int id, string rejectedById);
}
