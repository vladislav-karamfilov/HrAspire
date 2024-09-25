namespace HrAspire.Web.ApiGateway.Mappers;

using Google.Protobuf.WellKnownTypes;

using HrAspire.Vacations.Web;
using HrAspire.Web.Common;
using HrAspire.Web.Common.Models.VacationRequests;

using Riok.Mapperly.Abstractions;

[Mapper]
internal static partial class VacationRequestMapper
{
    public static partial VacationRequestResponseModel MapToResponseModel(this VacationRequestModel vacationRequest);

    public static partial VacationRequestDetailsResponseModel MapToDetailsResponseModel(this VacationRequestDetailsModel vacationRequest);

    private static DateTime TimestampToDateTime(Timestamp timestamp) => timestamp.ToDateTime();

    private static DateOnly TimestampToDateOnly(Timestamp timestamp) => timestamp.ToDateOnly();
}
