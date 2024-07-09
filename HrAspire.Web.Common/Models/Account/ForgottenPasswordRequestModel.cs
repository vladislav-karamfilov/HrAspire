namespace HrAspire.Web.Common.Models.Account;

using System.ComponentModel.DataAnnotations;

public class ForgottenPasswordRequestModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = default!;
}
