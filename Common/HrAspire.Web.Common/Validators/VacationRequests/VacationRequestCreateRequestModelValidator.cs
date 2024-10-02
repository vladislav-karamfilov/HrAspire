namespace HrAspire.Web.Common.Validators.VacationRequests;

using FluentValidation;

using HrAspire.Web.Common.Models.VacationRequests;

public class VacationRequestCreateRequestModelValidator : AbstractValidator<VacationRequestCreateRequestModel>
{
    public VacationRequestCreateRequestModelValidator()
    {
        this.RuleFor(m => m.Type).NotNull();
        this.RuleFor(m => m.FromDate).NotNull().GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today));
        this.RuleFor(m => m.ToDate).NotNull().GreaterThanOrEqualTo(m => m.FromDate);
    }
}
