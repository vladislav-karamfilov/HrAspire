namespace HrAspire.Web.Client.Models.Accounts;

using System.ComponentModel.DataAnnotations;

public class LoginInputModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = default!;

    [Required]
    [MinLength(6, ErrorMessage = "{0} must be at least {1} characters long.")] // TODO: const
    public string Password { get; set; } = default!;
}
