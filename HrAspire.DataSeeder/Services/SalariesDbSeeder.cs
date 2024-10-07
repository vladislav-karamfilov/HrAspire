namespace HrAspire.DataSeeder.Services;

using HrAspire.Employees.Data;
using HrAspire.Salaries.Business.SalaryRequests;
using HrAspire.Salaries.Data;

using Microsoft.EntityFrameworkCore;

public class SalariesDbSeeder
{
    private readonly ISalaryRequestsService salaryRequestsService;
    private readonly EmployeesDbContext employeesDbContext;
    private readonly ILogger<SalariesDbSeeder> logger;

    public SalariesDbSeeder(
        ISalaryRequestsService salaryRequestsService,
        EmployeesDbContext employeesDbContext,
        ILogger<SalariesDbSeeder> logger)
    {
        this.salaryRequestsService = salaryRequestsService;
        this.employeesDbContext = employeesDbContext;
        this.logger = logger;
    }

    public async Task SeedSalaryRequestsAsync(string managerId, string[] employeeIds)
    {
        var employeeIdsList = employeeIds.ToList();

        var salaryRequestsToCreate = employeeIds.Length / 2;
        for (var i = 0; i < salaryRequestsToCreate; i++)
        {
            var employeeIndex = Random.Shared.Next(employeeIdsList.Count);

            var employeeId = employeeIdsList[employeeIndex];

            employeeIdsList.RemoveAt(employeeIndex);

            var currentEmployeeSalary = await this.employeesDbContext.Employees
                .Where(e => e.Id == employeeId)
                .Select(e => e.Salary)
                .FirstOrDefaultAsync();

            var salaryRequestResult = await this.salaryRequestsService.CreateAsync(
                employeeId,
                currentEmployeeSalary + 1_000,
                notes: null,
                managerId);

            if (salaryRequestResult.IsError)
            {
                throw new Exception("Error creating salary request: " + salaryRequestResult.ErrorMessage);
            }
        }
    }
}
