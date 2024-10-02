namespace HrAspire.Web.Common.Validators.Account;

using FluentValidation;

using HrAspire.Web.Common.Models.Account;

public class LoginRequestModelValidator : AbstractValidator<LoginRequestModel>
{
    public LoginRequestModelValidator()
    {
        this.RuleFor(m => m.Email).NotEmpty().EmailAddress();
        this.RuleFor(m => m.Password).NotEmpty();
    }
}
