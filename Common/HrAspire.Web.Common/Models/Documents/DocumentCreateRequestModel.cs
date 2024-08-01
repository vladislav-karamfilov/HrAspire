namespace HrAspire.Web.Common.Models.Employees;

using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Http;

public class DocumentCreateRequestModel
{
    [Required]
    public string Title { get; set; } = default!;

    public string? Description { get; set; }

    [Required]
    public IFormFile File { get; set; } = default!;
}
