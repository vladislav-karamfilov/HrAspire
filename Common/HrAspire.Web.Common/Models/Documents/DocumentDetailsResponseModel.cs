namespace HrAspire.Web.Common.Models.Documents;

public record DocumentDetailsResponseModel(
    int Id,
    string EmployeeId,
    string Title,
    string? Description,
    string FileName,
    string CreatedById,
    string CreatedByFullName,
    DateTime CreatedOn);
