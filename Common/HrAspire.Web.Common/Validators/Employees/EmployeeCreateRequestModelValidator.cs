namespace HrAspire.Web.Common.Validators.Employees;

using FluentValidation;

using HrAspire.Web.Common.Models.Employees;

public class EmployeeCreateRequestModelValidator : AbstractValidator<EmployeeCreateRequestModel>
{
    public EmployeeCreateRequestModelValidator()
    {
        this.RuleFor(m => m.Email).NotEmpty().EmailAddress();
        this.RuleFor(m => m.Password).NotEmpty().MinimumLength(AccountConstants.PasswordMinLength);
        this.RuleFor(m => m.FullName).NotEmpty();
        this.RuleFor(m => m.Salary).GreaterThanOrEqualTo(0);
        this.RuleFor(m => m.Position).NotEmpty();
        this.RuleFor(m => m.DateOfBirth)
            .NotNull()
            .InclusiveBetween(DateOnly.FromDateTime(DateTime.Today.AddYears(-100)), DateOnly.FromDateTime(DateTime.Today.AddYears(-16)));
    }
}
