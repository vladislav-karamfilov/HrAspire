namespace HrAspire.Employees.Business.Employees;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Transactions;

using HrAspire.Business.Common;
using HrAspire.Employees.Data;
using HrAspire.Employees.Data.Models;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

public class EmployeesService : IEmployeesService
{
    private readonly EmployeesDbContext dbContext;
    private readonly UserManager<Employee> userManager;
    private readonly TimeProvider timeProvider;

    public EmployeesService(EmployeesDbContext dbContext, UserManager<Employee> userManager, TimeProvider timeProvider)
    {
        this.dbContext = dbContext;
        this.userManager = userManager;
        this.timeProvider = timeProvider;
    }

    public async Task<ServiceResult<string>> CreateAsync(
        string email,
        string password,
        string fullName,
        DateOnly dateOfBirth,
        string position,
        string? department,
        string? managerId,
        string role,
        string createdById)
    {
        var employee = new Employee
        {
            UserName = Guid.NewGuid().ToString(),
            Email = email,
            FullName = fullName,
            DateOfBirth = dateOfBirth,
            Position = position,
            Department = department,
            ManagerId = managerId,
            CreatedById = createdById,
            CreatedOn = this.timeProvider.GetUtcNow().UtcDateTime,
        };

        using var tx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        var result = await this.userManager.CreateAsync(employee, password);
        if (result.Succeeded && !string.IsNullOrWhiteSpace(role))
        {
            result = await this.userManager.AddToRoleAsync(employee, role);
        }

        if (!result.Succeeded)
        {
            return ServiceResult<string>.Error(result.GetFirstError()!);
        }

        tx.Complete();

        return ServiceResult<string>.Success(employee.Id);
    }

    public async Task<ServiceResult> UpdateAsync(
        string id,
        string fullName,
        DateOnly dateOfBirth,
        string position,
        string? department,
        string role,
        string? managerId)
    {
        var employee = await this.dbContext.Employees.FirstOrDefaultAsync(e => e.Id == id);
        if (employee is null)
        {
            return ServiceResult.ErrorNotFound;
        }

        employee.FullName = fullName;
        employee.DateOfBirth = dateOfBirth;
        employee.Position = position;
        employee.Department = department;
        employee.ManagerId = managerId;

        var currentRole = await this.dbContext.UserRoles.Where(ur => ur.UserId == id).Select(ur => ur.RoleId).FirstOrDefaultAsync();

        using var tx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        if (currentRole != role)
        {
            var identityResult = string.IsNullOrWhiteSpace(currentRole)
                ? IdentityResult.Success
                : await this.userManager.RemoveFromRoleAsync(employee, currentRole);

            if (identityResult.Succeeded && !string.IsNullOrWhiteSpace(role))
            {
                identityResult = await this.userManager.AddToRoleAsync(employee, role);
            }

            if (!identityResult.Succeeded)
            {
                return ServiceResult.Error(identityResult.GetFirstError()!);
            }
        }

        await this.dbContext.SaveChangesAsync();

        return ServiceResult.Success;
    }

    public async Task<ServiceResult> DeleteAsync(string id)
    {
        var deletedCount = await this.dbContext.Employees
            .Where(e => e.Id == id)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(e => e.IsDeleted, true)
                .SetProperty(e => e.DeletedOn, this.timeProvider.GetUtcNow().UtcDateTime));

        return deletedCount > 0 ? ServiceResult.Success : ServiceResult.ErrorNotFound;
    }

    public Task<EmployeeDetailsServiceModel?> GetEmployeeAsync(string id)
        => this.dbContext.Employees.Where(e => e.Id == id).ProjectToDetailsServiceModel().FirstOrDefaultAsync();

    public async Task<int> GetEmployeesCountAsync(string currentEmployeeId)
    {
        var isHrManager = await this.dbContext.UserRoles.AnyAsync(
            ur => ur.UserId == currentEmployeeId && ur.RoleId == BusinessConstants.HrManagerRole);

        var query = this.dbContext.Employees.AsQueryable();
        if (!isHrManager)
        {
            query = query.Where(e => e.ManagerId == currentEmployeeId);
        }

        return await query.CountAsync();
    }

    public async Task<IEnumerable<EmployeeServiceModel>> GetEmployeesAsync(string currentEmployeeId, int pageNumber, int pageSize)
    {
        pageNumber = Math.Max(pageNumber, 0);

        if (pageSize <= 0)
        {
            pageSize = 10;
        }
        else if (pageSize > 100)
        {
            pageSize = 100;
        }

        var isHrManager = await this.dbContext.UserRoles.AnyAsync(
            ur => ur.UserId == currentEmployeeId && ur.RoleId == BusinessConstants.HrManagerRole);

        var query = this.dbContext.Employees.AsQueryable();
        if (!isHrManager)
        {
            query = query.Where(e => e.ManagerId == currentEmployeeId);
        }

        return await query
            .OrderByDescending(e => e.CreatedOn)
            .Skip(pageNumber * pageSize)
            .Take(pageSize)
            .ProjectToServiceModel()
            .ToListAsync();
    }

    public async Task<IEnumerable<EmployeeServiceModel>> GetManagersAsync()
        => await this.dbContext.Employees
            .Where(e => e.Roles.Any(ur => ur.RoleId == BusinessConstants.ManagerRole))
            .ProjectToServiceModel()
            .ToListAsync();
}
