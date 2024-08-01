namespace HrAspire.Web.Common.Models.Documents;

using System.ComponentModel.DataAnnotations;

public class DocumentUpdateRequestModel
{
    [Required]
    public string Title { get; set; } = default!;

    public string? Description { get; set; }

    public byte[]? FileContent { get; set; }

    public string? FileName { get; set; }
}