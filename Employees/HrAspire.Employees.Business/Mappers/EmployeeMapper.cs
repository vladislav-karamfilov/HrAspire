namespace HrAspire.Employees.Business.Employees;

using HrAspire.Employees.Data.Models;

using Riok.Mapperly.Abstractions;

[Mapper]
internal static partial class EmployeeMapper
{
    public static partial IQueryable<EmployeeServiceModel> ProjectToServiceModel(this IQueryable<Employee> employeesQuery);

    public static partial IQueryable<EmployeeDetailsServiceModel> ProjectToDetailsServiceModel(this IQueryable<Employee> employeesQuery);
}
