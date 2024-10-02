namespace HrAspire.Web.Common.Validators.SalaryRequests;

using FluentValidation;

using HrAspire.Web.Common.Models.SalaryRequests;

public class SalaryRequestCreateRequestModelValidator : AbstractValidator<SalaryRequestCreateRequestModel>
{
    public SalaryRequestCreateRequestModelValidator()
    {
        this.RuleFor(m => m.NewSalary).GreaterThanOrEqualTo(0);
    }
}
