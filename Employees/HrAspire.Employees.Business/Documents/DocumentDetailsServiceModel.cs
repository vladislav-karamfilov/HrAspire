namespace HrAspire.Employees.Business.Documents;

public record DocumentDetailsServiceModel(string Title, string? Description, string Url, string CreatedByFullName, DateTime CreatedOn);
