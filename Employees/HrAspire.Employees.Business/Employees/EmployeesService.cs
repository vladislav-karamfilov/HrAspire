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

        var result = await this.userManager.CreateAsync(employee, password);
        if (result.Succeeded)
        {
            return ServiceResult<string>.Success(employee.Id);
        }

        var errorMessage = result.Errors.Select(e => e.Description).FirstOrDefault(e => !string.IsNullOrWhiteSpace(e))!;
        return ServiceResult<string>.Error(errorMessage);
    }

    public async Task<ServiceResult> UpdateAsync(
        string id,
        string fullName,
        DateOnly dateOfBirth,
        string position,
        string? department,
        string? managerId)
    {
        var updatedCount = await this.dbContext.Employees
            .Where(e => e.Id == id)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(e => e.FullName, fullName)
                .SetProperty(e => e.DateOfBirth, dateOfBirth)
                .SetProperty(e => e.Position, position)
                .SetProperty(e => e.Department, department)
                .SetProperty(e => e.ManagerId, managerId));

        return updatedCount > 0 ? ServiceResult.Success : ServiceResult.ErrorNotFound;
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

    public Task<int> GetEmployeesCountAsync() => this.dbContext.Employees.CountAsync();

    public async Task<IEnumerable<EmployeeServiceModel>> GetEmployeesAsync(int pageNumber, int pageSize)
    {
        if (pageNumber < 0)
        {
            pageNumber = 0;
        }

        if (pageSize <= 0)
        {
            pageSize = 10;
        }
        else if (pageSize > 100)
        {
            pageSize = 100;
        }

        return await this.dbContext.Employees
            .OrderByDescending(e => e.CreatedOn)
            .Skip(pageNumber * pageSize)
            .Take(pageSize)
            .ProjectToServiceModel()
            .ToListAsync();
    }
}
