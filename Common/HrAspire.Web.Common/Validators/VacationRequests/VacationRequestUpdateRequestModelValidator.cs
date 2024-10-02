namespace HrAspire.Web.Common.Validators.VacationRequests;

using FluentValidation;

using HrAspire.Web.Common.Models.VacationRequests;

public class VacationRequestUpdateRequestModelValidator : AbstractValidator<VacationRequestUpdateRequestModel>
{
    public VacationRequestUpdateRequestModelValidator()
    {
        this.RuleFor(m => m.Type).NotNull();
        this.RuleFor(m => m.FromDate).GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today));
        this.RuleFor(m => m.ToDate).GreaterThanOrEqualTo(m => m.FromDate);
    }
}
