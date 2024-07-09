namespace HrAspire.Web.Client.Models.Account;

using System.ComponentModel.DataAnnotations;

using HrAspire.Common;

public class LoginInputModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = default!;

    [Required]
    [MinLength(GlobalConstants.PasswordMinLength, ErrorMessage = "{0} must be at least {1} characters long.")]
    public string Password { get; set; } = default!;
}
