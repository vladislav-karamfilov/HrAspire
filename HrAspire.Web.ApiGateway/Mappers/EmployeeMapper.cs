namespace HrAspire.Web.ApiGateway.Mappers;

using Google.Protobuf.WellKnownTypes;

using HrAspire.Employees.Web;
using HrAspire.Web.Common;
using HrAspire.Web.Common.Models.Employees;

using Riok.Mapperly.Abstractions;

[Mapper]
internal static partial class EmployeeMapper
{
    public static partial EmployeeResponseModel MapToResponseModel(this PageEmployee pageEmployee);

    public static partial EmployeeDetailsResponseModel MapToDetailsResponseModel(this EmployeeDetails employeeDetails);

    private static DateTime TimestampToDateTime(Timestamp timestamp) => timestamp.ToDateTime();

    private static DateOnly TimestampToDate(Timestamp timestamp) => timestamp.ToDateOnly();
}
