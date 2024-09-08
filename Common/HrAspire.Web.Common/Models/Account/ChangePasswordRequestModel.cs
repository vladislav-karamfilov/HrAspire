namespace HrAspire.Web.Common.Models.Account;

using System.ComponentModel.DataAnnotations;

public class ChangePasswordRequestModel
{
    [Required]
    public string OldPassword { get; set; } = default!;

    [Required]
    [MinLength(AccountConstants.PasswordMinLength, ErrorMessage = "{0} must be at least {1} characters long.")]
    public string NewPassword { get; set; } = default!;

    [Compare(nameof(NewPassword))]
    public string ConfirmNewPassword { get; set; } = default!;
}
