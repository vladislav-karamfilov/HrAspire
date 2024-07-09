namespace HrAspire.Web.Common.Models.Account;

using System.ComponentModel.DataAnnotations;

public class LoginRequestModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = default!;

    [Required]
    [MinLength(AccountConstants.PasswordMinLength, ErrorMessage = "{0} must be at least {1} characters long.")]
    public string Password { get; set; } = default!;
}
