namespace HrAspire.Web.Common.Models.Documents;
public class DocumentUpdateRequestModel
{
    public string Title { get; set; } = default!;

    public string? Description { get; set; }

    public byte[]? FileContent { get; set; }

    public string? FileName { get; set; }
}