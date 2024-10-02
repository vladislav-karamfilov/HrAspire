namespace HrAspire.Web.Common.Validators.Documents;

using FluentValidation;

using HrAspire.Web.Common.Models.Documents;

public class DocumentCreateRequestModelValidator : AbstractValidator<DocumentCreateRequestModel>
{
    public DocumentCreateRequestModelValidator()
    {
        this.RuleFor(m => m.Title).NotEmpty();
        this.RuleFor(m => m.FileName).NotEmpty();
        this.RuleFor(m => m.FileContent).NotEmpty();
    }
}
