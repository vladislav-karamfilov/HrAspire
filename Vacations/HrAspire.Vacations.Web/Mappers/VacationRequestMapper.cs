namespace HrAspire.Vacations.Web.Mappers;

extern alias webcommon;

using Google.Protobuf.WellKnownTypes;

using HrAspire.Vacations.Business.VacationRequests;

using Riok.Mapperly.Abstractions;

using webcommon::HrAspire.Web.Common;

[Mapper]
internal static partial class VacationRequestMapper
{
    public static partial VacationRequestModel MapToVacationRequestGrpcModel(this VacationRequestServiceModel vacationRequest);

    public static partial VacationRequestDetailsModel MapToVacationRequestDetailsGrpcModel(
        this VacationRequestDetailsServiceModel vacationRequest);

    private static Timestamp DateTimeToTimestamp(DateTime dateTime) => dateTime.ToTimestamp();

    private static Timestamp DateOnlyToTimestamp(DateOnly dateOnly) => dateOnly.ToTimestamp();
}
