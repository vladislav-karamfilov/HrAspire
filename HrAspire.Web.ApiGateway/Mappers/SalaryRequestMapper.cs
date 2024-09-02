namespace HrAspire.Web.ApiGateway.Mappers;

using Google.Protobuf.WellKnownTypes;

using HrAspire.Salaries.Web;
using HrAspire.Web.Common.Models.SalaryRequests;

using Riok.Mapperly.Abstractions;

[Mapper]
internal static partial class SalaryRequestMapper
{
    public static partial SalaryRequestResponseModel MapToResponseModel(this SalaryRequestModel salaryRequest);

    public static partial SalaryRequestDetailsResponseModel MapToDetailsResponseModel(this SalaryRequestDetailsModel salaryRequest);

    private static DateTime TimestampToDateTime(Timestamp timestamp) => timestamp.ToDateTime();
}
