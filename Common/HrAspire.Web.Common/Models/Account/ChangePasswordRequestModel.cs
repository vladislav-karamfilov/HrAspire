namespace HrAspire.Web.Common.Models.Account;

public class ChangePasswordRequestModel
{
    public string OldPassword { get; set; } = default!;

    public string NewPassword { get; set; } = default!;

    public string ConfirmNewPassword { get; set; } = default!;
}
