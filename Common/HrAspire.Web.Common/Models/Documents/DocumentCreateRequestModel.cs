namespace HrAspire.Web.Common.Models.Documents;

using System.ComponentModel.DataAnnotations;

public class DocumentCreateRequestModel
{
    [Required]
    public string Title { get; set; } = default!;

    public string? Description { get; set; }

    [Required]
    public byte[] FileContent { get; set; } = default!;

    [Required]
    public string FileName { get; set; } = default!;
}
