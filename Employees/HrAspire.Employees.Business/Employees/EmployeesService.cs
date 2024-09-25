namespace HrAspire.Employees.Business.Employees;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using HrAspire.Business.Common;
using HrAspire.Employees.Data;
using HrAspire.Employees.Data.Models;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using StackExchange.Redis;

public class EmployeesService : IEmployeesService
{
    private readonly EmployeesDbContext dbContext;
    private readonly UserManager<Employee> userManager;
    private readonly IConnectionMultiplexer cacheConnectionMultiplexer;
    private readonly TimeProvider timeProvider;

    public EmployeesService(
        EmployeesDbContext dbContext,
        UserManager<Employee> userManager,
        IConnectionMultiplexer cacheConnectionMultiplexer,
        TimeProvider timeProvider)
    {
        this.dbContext = dbContext;
        this.userManager = userManager;
        this.cacheConnectionMultiplexer = cacheConnectionMultiplexer;
        this.timeProvider = timeProvider;
    }

    private IDatabase CacheDatabase => this.cacheConnectionMultiplexer.GetDatabase();

    public async Task<ServiceResult<string>> CreateAsync(
        string email,
        string password,
        string fullName,
        DateOnly dateOfBirth,
        string position,
        string? department,
        string? managerId,
        decimal salary,
        string role,
        string createdById)
    {
        var employee = new Employee
        {
            UserName = Guid.NewGuid().ToString("N"),
            Email = email,
            FullName = fullName,
            DateOfBirth = dateOfBirth,
            Position = position,
            Department = department,
            ManagerId = managerId,
            Salary = salary,
            CreatedById = createdById,
            CreatedOn = this.timeProvider.GetUtcNow().UtcDateTime,
        };

        if (!string.IsNullOrWhiteSpace(role))
        {
            employee.Roles.Add(new IdentityUserRole<string> { RoleId = role, UserId = employee.Id });
        }

        await this.CacheDatabase.HashSetAsync(BusinessConstants.EmployeeNamesCacheSetName, employee.Id, employee.FullName);

        var identityResult = await this.userManager.CreateAsync(employee, password);
        if (!identityResult.Succeeded)
        {
            try
            {
                await this.CacheDatabase.HashDeleteAsync(BusinessConstants.EmployeeNamesCacheSetName, employee.Id);
            }
            catch
            {
                // It's not crucial to delete the hash key from cache so ignore exceptions here
            }

            return ServiceResult<string>.Error(identityResult.GetFirstError()!);
        }

        return ServiceResult<string>.Success(employee.Id);
    }

    public async Task<ServiceResult> UpdateAsync(
        string id,
        string fullName,
        DateOnly dateOfBirth,
        string position,
        string? department,
        string? managerId,
        string role)
    {
        var employee = await this.dbContext.Employees.FirstOrDefaultAsync(e => e.Id == id);
        if (employee is null)
        {
            return ServiceResult.ErrorNotFound;
        }

        if (id == managerId)
        {
            return ServiceResult.Error("Employee cannot be manager of themselves.");
        }

        var oldEmployeeFullName = employee.FullName;
        employee.FullName = fullName;
        employee.DateOfBirth = dateOfBirth;
        employee.Position = position;
        employee.Department = department;
        employee.ManagerId = managerId;

        var currentRole = await this.dbContext.UserRoles.Where(ur => ur.UserId == id).Select(ur => ur.RoleId).FirstOrDefaultAsync();

        string? errorMessage = null;

        try
        {
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

                await this.CacheDatabase.HashSetAsync(BusinessConstants.EmployeeNamesCacheSetName, employee.Id, employee.FullName);

                await tx.CommitAsync();
            });
        }
        catch (Exception ex) when (ex is not RedisException)
        {
            // Set the old employee full name just in case it's was updated before the exception
            await this.CacheDatabase.HashSetAsync(BusinessConstants.EmployeeNamesCacheSetName, employee.Id, oldEmployeeFullName);

            throw;
        }

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

    public async Task<ServiceResult> UpdateSalaryAsync(string id, decimal newSalary)
    {
        if (newSalary < 0)
        {
            return ServiceResult.Error("New employee salary cannot be negative");
        }

        var updatedCount = await this.dbContext.Employees
            .Where(e => e.Id == id)
            .ExecuteUpdateAsync(setters => setters.SetProperty(e => e.Salary, newSalary));

        return updatedCount > 0 ? ServiceResult.Success : ServiceResult.ErrorNotFound;
    }

    public async Task<ServiceResult> UpdateUsedPaidVacationDaysAsync(string id, int usedPaidVacationDays)
    {
        if (usedPaidVacationDays < 0)
        {
            return ServiceResult.Error("New employee used paid vacation days cannot be negative");
        }

        var updatedCount = await this.dbContext.Employees
            .Where(e => e.Id == id)
            .ExecuteUpdateAsync(setters => setters.SetProperty(e => e.UsedPaidVacationDays, usedPaidVacationDays));

        return updatedCount > 0 ? ServiceResult.Success : ServiceResult.ErrorNotFound;
    }
}
