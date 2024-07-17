namespace HrAspire.Web.Common.Mappers;

using HrAspire.Web.Common.Models.Employees;

using Riok.Mapperly.Abstractions;

[Mapper]
public static partial class EmployeeMapper
{
    public static partial EmployeeUpdateRequestModel ToUpdateRequestModel(this EmployeeDetailsResponseModel employee);
}
