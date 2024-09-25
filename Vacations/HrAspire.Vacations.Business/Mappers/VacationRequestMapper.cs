namespace HrAspire.Vacations.Business.Mappers;

using HrAspire.Vacations.Business.VacationRequests;
using HrAspire.Vacations.Data.Models;

using Riok.Mapperly.Abstractions;

[Mapper]
internal static partial class VacationRequestMapper
{
    public static partial IQueryable<VacationRequestServiceModel> ProjectToServiceModel(
        this IQueryable<VacationRequest> vacationRequestsQuery);

    public static partial IQueryable<VacationRequestDetailsServiceModel> ProjectToDetailsServiceModel(
        this IQueryable<VacationRequest> vacationRequestsQuery);
}
