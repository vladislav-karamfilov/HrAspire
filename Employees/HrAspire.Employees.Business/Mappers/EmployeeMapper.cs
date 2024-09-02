namespace HrAspire.Employees.Business.Employees;

using HrAspire.Employees.Data.Models;

using Microsoft.AspNetCore.Identity;

using Riok.Mapperly.Abstractions;

[Mapper]
internal static partial class EmployeeMapper
{
    public static partial IQueryable<EmployeeServiceModel> ProjectToServiceModel(this IQueryable<Employee> employeesQuery);

    public static partial IQueryable<EmployeeDetailsServiceModel> ProjectToDetailsServiceModel(this IQueryable<Employee> employeesQuery);

    [MapProperty(nameof(Employee.Roles), nameof(EmployeeDetailsServiceModel.Role), Use = nameof(MapRole))]
    private static partial EmployeeDetailsServiceModel MapToDetailsServiceModel(Employee employee);

    private static string? MapRole(ICollection<IdentityUserRole<string>> userRoles) => userRoles.Select(ur => ur.RoleId).FirstOrDefault();
}
