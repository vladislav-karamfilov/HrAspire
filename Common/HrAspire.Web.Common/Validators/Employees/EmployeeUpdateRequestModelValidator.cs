namespace HrAspire.Web.Common.Validators.Employees;

using FluentValidation;

using HrAspire.Web.Common.Models.Employees;

public class EmployeeUpdateRequestModelValidator : AbstractValidator<EmployeeUpdateRequestModel>
{
    public EmployeeUpdateRequestModelValidator()
    {
        this.RuleFor(m => m.FullName).NotEmpty();
        this.RuleFor(m => m.Position).NotEmpty();
        this.RuleFor(m => m.DateOfBirth)
            .InclusiveBetween(DateOnly.FromDateTime(DateTime.Today.AddYears(-100)), DateOnly.FromDateTime(DateTime.Today.AddYears(-16)));
    }
}
