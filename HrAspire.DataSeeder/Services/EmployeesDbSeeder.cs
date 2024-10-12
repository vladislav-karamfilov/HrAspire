namespace HrAspire.DataSeeder.Services;

using Bogus;

using HrAspire.Business.Common;
using HrAspire.Employees.Business.Documents;
using HrAspire.Employees.Business.Employees;
using HrAspire.Employees.Data;
using HrAspire.Employees.Data.Models;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

public class EmployeesDbSeeder
{
    private const string UserPassword = "Password1!";

    private readonly IEmployeesService employeesService;
    private readonly IDocumentsService documentsService;
    private readonly EmployeesDbContext dbContext;
    private readonly ILogger<EmployeesDbSeeder> logger;

    public EmployeesDbSeeder(
        IEmployeesService employeesService,
        IDocumentsService documentsService,
        EmployeesDbContext dbContext,
        ILogger<EmployeesDbSeeder> logger)
    {
        this.employeesService = employeesService;
        this.documentsService = documentsService;
        this.dbContext = dbContext;
        this.logger = logger;
    }

    public async Task<(string? HrManagerId, string? ManagerId, string[] EmployeeIds)> SeedEmployeesAsync()
    {
        if (await this.dbContext.Employees.AnyAsync())
        {
            return (null, null, []);
        }

        var employeesFaker = new Faker<Employee>()
            .RuleFor(e => e.FullName, f => f.Name.FullName())
            .RuleFor(
                e => e.DateOfBirth,
                f => f.Date.BetweenDateOnly(
                    DateOnly.FromDateTime(DateTime.Today.AddYears(-42)),
                    DateOnly.FromDateTime(DateTime.Today.AddYears(-21))));

        var employees = employeesFaker.Generate(count: 10);

        var hrManager = employees[0];
        hrManager.Email = GenerateEmailFromFullName(hrManager.FullName);
        var hrManagerResult = await this.employeesService.CreateAsync(
            hrManager.Email,
            UserPassword,
            hrManager.FullName,
            hrManager.DateOfBirth,
            position: "HR Manager",
            department: "HR",
            managerId: null,
            salary: 6_000,
            BusinessConstants.HrManagerRole,
            createdById: default!);

        if (hrManagerResult.IsError)
        {
            throw new Exception($"Error creating HR manager: {hrManagerResult.ErrorMessage}");
        }

        hrManager.Id = hrManagerResult.Data!;

        this.logger.LogInformation("Created HR Manager employee: {Email}", hrManager.Email);

        var teamLead = employees[1];
        teamLead.Email = GenerateEmailFromFullName(teamLead.FullName);
        var teamLeadResult = await this.employeesService.CreateAsync(
            teamLead.Email,
            UserPassword,
            teamLead.FullName,
            teamLead.DateOfBirth,
            position: "Team Lead",
            department: "IT",
            managerId: null,
            salary: 10_000,
            BusinessConstants.ManagerRole,
            createdById: hrManager.Id);

        if (teamLeadResult.IsError)
        {
            throw new Exception($"Error creating team lead: {teamLeadResult.ErrorMessage}");
        }

        teamLead.Id = teamLeadResult.Data!;

        this.logger.LogInformation("Created Team Lead employee: {Email}", teamLead.Email);

        for (var i = 2; i < employees.Count; i++)
        {
            var position = i switch
            {
                < 5 => "Senior Software Engineer",
                < 7 => "Software Engineer",
                _ => "Junior Software Engineer",
            };

            var salary = i switch
            {
                < 5 => 7_500,
                < 7 => 5_000,
                _ => 2_500,
            };

            var employee = employees[i];
            employee.Email = GenerateEmailFromFullName(employee.FullName);
            var employeeResult = await this.employeesService.CreateAsync(
                employee.Email,
                UserPassword,
                employee.FullName,
                employee.DateOfBirth,
                position,
                department: "IT",
                managerId: teamLead.Id,
                salary: salary,
                role: null,
                createdById: hrManager.Id);

            if (employeeResult.IsError)
            {
                throw new Exception($"Error creating employee: {employeeResult.ErrorMessage}");
            }

            employee.Id = employeeResult.Data!;

            this.logger.LogInformation("Created {Position} employee: {Email}", position, employee.Email);
        }

        return (hrManager.Id, teamLead.Id, employees.Skip(2).Select(e => e.Id).ToArray());
    }

    public async Task SeedDocumentsAsync(string hrManagerId, IEnumerable<string> employeeIds)
    {
        var sampleDocumentFilePath = "./id-card.jpg";
        var fileBytes = File.ReadAllBytes(sampleDocumentFilePath);
        var fileName = Path.GetFileName(sampleDocumentFilePath);

        foreach (var employeeId in employeeIds)
        {
            var documentResult =
                await this.documentsService.CreateAsync(employeeId, "ID Card", description: null, fileBytes, fileName, hrManagerId);

            if (documentResult.IsError)
            {
                throw new Exception($"Error creating document: {documentResult.ErrorMessage}");
            }
        }
    }

    public async Task SeedRolesAsync()
    {
        if (await this.dbContext.Roles.AnyAsync())
        {
            return;
        }

        this.dbContext.Roles.Add(new IdentityRole
        {
            Id = BusinessConstants.ManagerRole,
            Name = BusinessConstants.ManagerRole,
            NormalizedName = BusinessConstants.ManagerRole.ToUpperInvariant(),
            ConcurrencyStamp = Guid.NewGuid().ToString(),
        });

        this.dbContext.Roles.Add(new IdentityRole
        {
            Id = BusinessConstants.HrManagerRole,
            Name = BusinessConstants.HrManagerRole,
            NormalizedName = BusinessConstants.HrManagerRole.ToUpperInvariant(),
            ConcurrencyStamp = Guid.NewGuid().ToString(),
        });

        await this.dbContext.SaveChangesAsync();
    }

    private static string GenerateEmailFromFullName(string fullName)
        => $"{fullName.Replace(' ', '.').Replace("'", string.Empty).ToLowerInvariant()}@thecompany.com";
}
