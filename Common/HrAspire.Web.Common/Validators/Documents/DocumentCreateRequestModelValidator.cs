namespace HrAspire.Web.Common.Validators.Documents;

using FluentValidation;

using HrAspire.Business.Common;
using HrAspire.Web.Common.Models.Documents;

public class DocumentCreateRequestModelValidator : AbstractValidator<DocumentCreateRequestModel>
{
    public DocumentCreateRequestModelValidator()
    {
        this.RuleFor(m => m.Title).NotEmpty();
        this.RuleFor(m => m.FileName).NotEmpty();
        this.RuleFor(m => m.FileContent).NotEmpty();
        this.RuleFor(m => m.FileName)
            .Must(fileName =>
            {
                if (fileName is null)
                {
                    return true;
                }

                var extension = Path.GetExtension(fileName);
                return BusinessConstants.AllowedDocumentFileExtensions.Contains(extension);
            })
            .WithMessage(
                $"Only files with these extensions allowed: {string.Join(", ", BusinessConstants.AllowedDocumentFileExtensions)}");
    }
}
