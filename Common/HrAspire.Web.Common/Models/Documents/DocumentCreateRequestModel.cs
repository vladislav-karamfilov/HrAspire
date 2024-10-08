namespace HrAspire.Web.Common.Models.Documents;

public class DocumentCreateRequestModel
{
    public string Title { get; set; } = default!;

    public string? Description { get; set; }

    public byte[] FileContent { get; set; } = default!;

    public string FileName { get; set; } = default!;
}
