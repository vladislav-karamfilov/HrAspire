namespace HrAspire.Web.Common.Models.Documents;

public record DocumentResponseModel(int Id, string Title, string FileName, string CreatedById, DateTime CreatedOn);
