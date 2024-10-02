namespace HrAspire.Web.Common.Validators.Documents;

using FluentValidation;

using HrAspire.Web.Common.Models.Documents;

public class DocumentUpdateRequestModelValidator : AbstractValidator<DocumentUpdateRequestModel>
{
    public DocumentUpdateRequestModelValidator()
    {
        this.RuleFor(m => m.Title).NotEmpty();
    }
}
