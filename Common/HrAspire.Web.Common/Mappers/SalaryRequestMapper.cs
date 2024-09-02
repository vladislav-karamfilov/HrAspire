namespace HrAspire.Web.Common.Mappers;

using HrAspire.Web.Common.Models.SalaryRequests;

using Riok.Mapperly.Abstractions;

[Mapper]
public static partial class SalaryRequestMapper
{
    public static partial SalaryRequestUpdateRequestModel ToUpdateRequestModel(this SalaryRequestDetailsResponseModel salaryRequest);
}
