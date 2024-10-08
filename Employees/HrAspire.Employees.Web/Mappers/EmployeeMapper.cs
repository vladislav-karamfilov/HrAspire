﻿namespace HrAspire.Employees.Web.Mappers;

using Google.Protobuf.WellKnownTypes;

using HrAspire.Employees.Business.Employees;
using HrAspire.Web.Common;

using Riok.Mapperly.Abstractions;

[Mapper]
internal static partial class EmployeeMapper
{
    public static partial EmployeeModel MapToEmployeeGrpcModel(this EmployeeServiceModel employee);

    public static partial EmployeeDetailsModel MapToEmployeeDetailsGrpcModel(this EmployeeDetailsServiceModel employee);

    private static Timestamp DateOnlyToTimestamp(DateOnly dateOnly) => dateOnly.ToTimestamp();

    private static Timestamp DateTimeToTimestamp(DateTime dateTime) => dateTime.ToTimestamp();
}
