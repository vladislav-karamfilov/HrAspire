namespace HrAspire.Employees.Business.Employees;

using HrAspire.Employees.Data.Models;

using Microsoft.AspNetCore.Identity;

using Riok.Mapperly.Abstractions;

[Mapper]
internal static partial class EmployeeMapper
{
    public static partial IQueryable<EmployeeServiceModel> ProjectToServiceModel(this IQueryable<Employee> employeesQuery);

    public static partial IQueryable<EmployeeDetailsServiceModel> ProjectToDetailsServiceModel(this IQueryable<Employee> employeesQuery);

#pragma warning disable IDE0051 // Remove unused private members - used in the ProjectToDetailsServiceModel() above
    [MapProperty(nameof(Employee.Roles), nameof(EmployeeDetailsServiceModel.Role), Use = nameof(MapRole))]
    private static partial EmployeeDetailsServiceModel MapToDetailsServiceModel(Employee employee);
#pragma warning restore IDE0051 // Remove unused private members

    private static string? MapRole(ICollection<IdentityUserRole<string>> userRoles) => userRoles.Select(ur => ur.RoleId).FirstOrDefault();
}
