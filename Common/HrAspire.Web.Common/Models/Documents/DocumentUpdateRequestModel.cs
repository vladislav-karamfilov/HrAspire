namespace HrAspire.Web.Common.Models.Documents;

using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Http;

public class DocumentUpdateRequestModel
{
    [Required]
    public string Title { get; set; } = default!;

    public string? Description { get; set; }

    public IFormFile? File { get; set; }
}