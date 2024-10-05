namespace HrAspire.Employees.Business.Documents;

using HrAspire.Business.Common;

public interface IDocumentsService
{
    Task<IEnumerable<DocumentServiceModel>> ListEmployeeDocumentsAsync(string employeeId, int pageNumber, int pageSize, string? managerId);

    Task<int> GetEmployeeDocumentsCountAsync(string employeeId, string? managerId);

    Task<DocumentUrlAndFileNameServiceModel?> GetUrlAndFileNameAsync(int id, string? managerId);

    Task<DocumentDetailsServiceModel?> GetAsync(int id, string? managerId);

    Task<ServiceResult<int>> CreateAsync(
        string employeeId,
        string title,
        string? description,
        byte[] fileContent,
        string fileName,
        string createdById);

    Task<ServiceResult> UpdateAsync(int id, string title, string? description, byte[]? fileContent, string? fileName, string currentEmployeeId);

    Task<ServiceResult> DeleteAsync(int id, string currentEmployeeId);
}
