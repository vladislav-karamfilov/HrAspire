namespace HrAspire.Salaries.Business.Mappers;

using HrAspire.Salaries.Business.SalaryRequests;
using HrAspire.Salaries.Data.Models;

using Riok.Mapperly.Abstractions;

[Mapper]
internal static partial class SalaryRequestMapper
{
    public static partial IQueryable<SalaryRequestServiceModel> ProjectToServiceModel(
        this IQueryable<SalaryRequest> salaryRequestsQuery);

    public static partial IQueryable<SalaryRequestDetailsServiceModel> ProjectToDetailsServiceModel(
        this IQueryable<SalaryRequest> salaryRequestsQuery);
}
