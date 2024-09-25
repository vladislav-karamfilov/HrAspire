namespace HrAspire.Vacations.Web.Mappers;

using Google.Protobuf.WellKnownTypes;

using HrAspire.Vacations.Business.VacationRequests;
using HrAspire.Web.Common;

using Riok.Mapperly.Abstractions;

[Mapper]
internal static partial class VacationRequestMapper
{
    public static partial VacationRequestModel MapToVacationRequestGrpcModel(this VacationRequestServiceModel vacationRequest);

    public static partial VacationRequestDetailsModel MapToVacationRequestDetailsGrpcModel(
        this VacationRequestDetailsServiceModel vacationRequest);

    private static Timestamp DateTimeToTimestamp(DateTime dateTime) => dateTime.ToTimestamp();

    private static Timestamp DateOnlyToTimestamp(DateOnly dateOnly) => dateOnly.ToTimestamp();
}
