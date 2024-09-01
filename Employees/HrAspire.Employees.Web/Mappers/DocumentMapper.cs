namespace HrAspire.Employees.Web.Mappers;

using Google.Protobuf.WellKnownTypes;

using HrAspire.Employees.Business.Documents;
using HrAspire.Web.Common;

using Riok.Mapperly.Abstractions;

[Mapper]
internal static partial class DocumentMapper
{
    public static partial DocumentModel MapToDocumentGrpcModel(this DocumentServiceModel document);

    public static partial DocumentDetailsModel MapToDocumentDetailsGrpcModel(this DocumentDetailsServiceModel document);

    private static Timestamp DateTimeToTimestamp(DateTime dateTime) => dateTime.ToTimestamp();
}
