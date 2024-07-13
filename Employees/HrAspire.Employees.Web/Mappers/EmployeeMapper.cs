namespace HrAspire.Employees.Web.Mappers;

using Google.Protobuf.WellKnownTypes;

using HrAspire.Employees.Business.Employees;

using Riok.Mapperly.Abstractions;

[Mapper]
internal static partial class EmployeeMapper
{
    public static partial PageEmployee MapToPageEmployee(this EmployeeServiceModel employee);

    public static partial EmployeeDetails MapToEmployeeDetails(this EmployeeDetailsServiceModel employee);

    private static Timestamp DateOnlyToTimestamp(DateOnly dateOnly) => dateOnly.ToDateTime(default, DateTimeKind.Utc).ToTimestamp();

    private static Timestamp DateTimeToTimestamp(DateTime dateTime) => dateTime.ToTimestamp();
}
