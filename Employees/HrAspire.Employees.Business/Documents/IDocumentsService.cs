namespace HrAspire.Employees.Business.Documents;

using HrAspire.Business.Common;

public interface IDocumentsService
{
    Task<IEnumerable<DocumentServiceModel>> GetByEmployeeIdAsync(string employeeId, int pageNumber, int pageSize);

    Task<DocumentDetailsServiceModel?> GetDocumentAsync(int id);

    Task<ServiceResult<int>> CreateAsync(
        string employeeId,
        string title,
        string? description,
        byte[] fileContent,
        string fileName,
        string createdById);

    Task<ServiceResult> UpdateAsync(int id, string title, string? description, byte[]? fileContent, string? fileName);

    Task<ServiceResult> DeleteAsync(int id);
}
