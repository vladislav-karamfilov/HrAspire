namespace HrAspire.Web.ApiGateway.Mappers;

using Google.Protobuf.WellKnownTypes;

using HrAspire.Employees.Web;
using HrAspire.Web.Common.Models.Documents;

using Riok.Mapperly.Abstractions;

[Mapper]
internal static partial class DocumentMapper
{
    public static partial DocumentResponseModel MapToResponseModel(this DocumentModel document);

    public static partial DocumentDetailsResponseModel MapToDetailsResponseModel(this DocumentDetailsModel document);

    private static DateTime TimestampToDateTime(Timestamp timestamp) => timestamp.ToDateTime();
}
