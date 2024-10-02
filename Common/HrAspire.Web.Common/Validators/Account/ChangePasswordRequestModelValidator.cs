namespace HrAspire.Web.Common.Validators.Account;

using FluentValidation;

using HrAspire.Web.Common.Models.Account;

public class ChangePasswordRequestModelValidator : AbstractValidator<ChangePasswordRequestModel>
{
    public ChangePasswordRequestModelValidator()
    {
        this.RuleFor(m => m.OldPassword).NotEmpty();
        this.RuleFor(m => m.NewPassword).NotEmpty().MinimumLength(AccountConstants.PasswordMinLength);
        this.RuleFor(m => m.ConfirmNewPassword).Equal(m => m.NewPassword).WithMessage("'Confirm New Password' must match 'New Password'");
    }
}
