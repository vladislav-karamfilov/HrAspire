namespace HrAspire.Web.Common.Validators.SalaryRequests;

using FluentValidation;

using HrAspire.Web.Common.Models.SalaryRequests;

public class SalaryRequestUpdateRequestModelValidator : AbstractValidator<SalaryRequestUpdateRequestModel>
{
    public SalaryRequestUpdateRequestModelValidator()
    {
        this.RuleFor(m => m.NewSalary).GreaterThanOrEqualTo(0);
    }
}
