namespace HrAspire.Web.Common.Models.Documents;

public record DocumentDetailsResponseModel(
    int Id,
    string Title,
    string? Description,
    string FileName,
    string CreatedByFullName,
    DateTime CreatedOn);
