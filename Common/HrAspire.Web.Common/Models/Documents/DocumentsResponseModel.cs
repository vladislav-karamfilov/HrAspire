namespace HrAspire.Web.Common.Models.Documents;

public record DocumentsResponseModel(ICollection<DocumentResponseModel> Documents, int Total);
