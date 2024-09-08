namespace HrAspire.Web.Common.Models.Account;

using System.ComponentModel.DataAnnotations;

public class LoginRequestModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = default!;

    [Required]
    public string Password { get; set; } = default!;
}
