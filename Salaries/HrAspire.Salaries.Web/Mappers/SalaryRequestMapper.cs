namespace HrAspire.Salaries.Web.Mappers;

using Google.Protobuf.WellKnownTypes;

using HrAspire.Salaries.Business.SalaryRequests;

using Riok.Mapperly.Abstractions;

[Mapper]
internal static partial class SalaryRequestMapper
{
    public static partial SalaryRequestModel MapToSalaryRequestGrpcModel(this SalaryRequestServiceModel salaryRequest);

    public static partial SalaryRequestDetailsModel MapToSalaryRequestDetailsGrpcModel(
        this SalaryRequestDetailsServiceModel salaryRequest);

    private static Timestamp DateTimeToTimestamp(DateTime dateTime) => dateTime.ToTimestamp();
}
