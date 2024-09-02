namespace HrAspire.Employees.Business.Employees;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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

        string? errorMessage = null;

        await this.dbContext.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
        {
            await using var tx = await this.dbContext.Database.BeginTransactionAsync();

            var result = await this.userManager.CreateAsync(employee, password);
            if (result.Succeeded && !string.IsNullOrWhiteSpace(role))
            {
                result = await this.userManager.AddToRoleAsync(employee, role);
            }

            if (!result.Succeeded)
            {
                errorMessage = result.GetFirstError();
                return;
            }

            await tx.CommitAsync();
        });

        var result = string.IsNullOrWhiteSpace(errorMessage)
            ? ServiceResult<string>.Success(employee.Id)
            : ServiceResult<string>.Error(errorMessage);

        return result;
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

        string? errorMessage = null;

        await this.dbContext.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
        {
            await using var tx = await this.dbContext.Database.BeginTransactionAsync();

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
                    errorMessage = identityResult.GetFirstError();
                    return;
                }
            }

            await this.dbContext.SaveChangesAsync();

            await tx.CommitAsync();
        });

        var result = string.IsNullOrWhiteSpace(errorMessage)
            ? ServiceResult.Success
            : ServiceResult.Error(errorMessage);

        return result;
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

    public Task<EmployeeDetailsServiceModel?> GetAsync(string id)
        => this.dbContext.Employees.Where(e => e.Id == id).ProjectToDetailsServiceModel().FirstOrDefaultAsync();

    public async Task<int> GetCountAsync(string currentEmployeeId)
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

    public async Task<IEnumerable<EmployeeServiceModel>> ListAsync(string currentEmployeeId, int pageNumber, int pageSize)
    {
        PaginationHelper.Normalize(ref pageNumber, ref pageSize);

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

    public async Task<IEnumerable<EmployeeServiceModel>> ListManagersAsync()
        => await this.dbContext.Employees
            .Where(e => e.Roles.Any(ur => ur.RoleId == BusinessConstants.ManagerRole))
            .ProjectToServiceModel()
            .ToListAsync();
}
