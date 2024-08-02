namespace HrAspire.Employees.Business.Documents;

using HrAspire.Business.Common;

public interface IDocumentsService
{
    Task<IEnumerable<DocumentServiceModel>> GetEmployeeDocumentsAsync(string employeeId, int pageNumber, int pageSize);

    Task<int> GetEmployeeDocumentsCountAsync(string employeeId);

    Task<DocumentUrlAndFileNameServiceModel?> GetDocumentUrlAndFileNameAsync(int id, string employeeId);

    Task<DocumentDetailsServiceModel?> GetDocumentAsync(int id, string employeeId);

    Task<ServiceResult<int>> CreateAsync(
        string employeeId,
        string title,
        string? description,
        byte[] fileContent,
        string fileName,
        string createdById);

    Task<ServiceResult> UpdateAsync(int id, string employeeId, string title, string? description, byte[]? fileContent, string? fileName);

    Task<ServiceResult> DeleteAsync(int id, string employeeId);
}
