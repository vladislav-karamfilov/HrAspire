namespace HrAspire.Web.Common.Mappers;

using HrAspire.Web.Common.Models.VacationRequests;

using Riok.Mapperly.Abstractions;

[Mapper]
public static partial class VacationRequestMapper
{
    public static partial VacationRequestUpdateRequestModel ToUpdateRequestModel(this VacationRequestDetailsResponseModel vacationRequest);
}
