namespace HrAspire.Web.Common.Mappers;

using HrAspire.Web.Common.Models.Documents;

using Riok.Mapperly.Abstractions;

[Mapper]
public static partial class DocumentMapper
{
    public static partial DocumentUpdateRequestModel ToUpdateRequestModel(this DocumentDetailsResponseModel document);
}
