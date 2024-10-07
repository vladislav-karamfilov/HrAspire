namespace HrAspire.DataSeeder;

using System.Threading;
using System.Threading.Tasks;

using Bogus;

using HrAspire.Business.Common;
using HrAspire.Employees.Business.Employees;
using HrAspire.Employees.Data;
using HrAspire.Employees.Data.Models;
using HrAspire.Salaries.Data;
using HrAspire.Vacations.Data;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

public class DataSeederWorker : BackgroundService
{
    private readonly IServiceScopeFactory scopeFactory;
    private readonly IHostEnvironment hostEnvironment;
    private readonly IHostApplicationLifetime hostApplicationLifetime;
    private readonly ILogger<DataSeederWorker> logger;

    public DataSeederWorker(
        IServiceScopeFactory scopeFactory,
        IHostEnvironment hostEnvironment,
        IHostApplicationLifetime hostApplicationLifetime,
        ILogger<DataSeederWorker> logger)
    {
        this.scopeFactory = scopeFactory;
        this.hostEnvironment = hostEnvironment;
        this.hostApplicationLifetime = hostApplicationLifetime;
        this.logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (this.hostEnvironment.IsDevelopment())
        {
            await this.MigrateAndSeedEmployeesDbAsync(stoppingToken);

            await this.MigrateAndSeedSalariesDbAsync(stoppingToken);

            await this.MigrateAndSeedVacationsDbAsync(stoppingToken);
        }

        this.hostApplicationLifetime.StopApplication();
    }

    private static string GenerateEmailFromFullName(string fullName)
        => $"{fullName.Replace(' ', '.').Replace("'", string.Empty).ToLowerInvariant()}@thecompany.com";

    private async Task MigrateAndSeedEmployeesDbAsync(CancellationToken cancellationToken)
    {
        using var scope = this.scopeFactory.CreateScope();
        using var dbContext = scope.ServiceProvider.GetRequiredService<EmployeesDbContext>();
        var employeesService = scope.ServiceProvider.GetRequiredService<IEmployeesService>();

        this.logger.LogInformation("Migrating employees DB...");
        await dbContext.Database.MigrateAsync(cancellationToken);
        this.logger.LogInformation("Migrated employees DB.");

        // TODO: extract to separate method and seed more data - employees, documents(?)
        if (!await dbContext.Roles.AnyAsync())
        {
            dbContext.Roles.Add(new IdentityRole
            {
                Id = BusinessConstants.ManagerRole,
                Name = BusinessConstants.ManagerRole,
                NormalizedName = BusinessConstants.ManagerRole.ToUpperInvariant(),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
            });

            dbContext.Roles.Add(new IdentityRole
            {
                Id = BusinessConstants.HrManagerRole,
                Name = BusinessConstants.HrManagerRole,
                NormalizedName = BusinessConstants.HrManagerRole.ToUpperInvariant(),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
            });

            await dbContext.SaveChangesAsync();
        }

        // TODO:
        if (true || !await dbContext.Employees.AnyAsync())
        {
            var employees = await this.SeedEmployeesAsync(employeesService);
        }
    }

    private async Task MigrateAndSeedSalariesDbAsync(CancellationToken cancellationToken)
    {
        using var scope = this.scopeFactory.CreateScope();
        using var dbContext = scope.ServiceProvider.GetRequiredService<SalariesDbContext>();

        this.logger.LogInformation("Migrating salaries DB...");
        await dbContext.Database.MigrateAsync(cancellationToken);
        this.logger.LogInformation("Migrated salaries DB.");

        // TODO: seed data - salary requests?
    }

    private async Task MigrateAndSeedVacationsDbAsync(CancellationToken cancellationToken)
    {
        using var scope = this.scopeFactory.CreateScope();
        using var dbContext = scope.ServiceProvider.GetRequiredService<VacationsDbContext>();

        this.logger.LogInformation("Migrating vacations DB...");
        await dbContext.Database.MigrateAsync(cancellationToken);
        this.logger.LogInformation("Migrated vacations DB.");

        // TODO: seed data - vacation requests?
    }

    private async Task<IReadOnlyList<Employee>> SeedEmployeesAsync(IEmployeesService employeesService)
    {
        var employeesFaker = new Faker<Employee>()
            .RuleFor(e => e.FullName, p => p.Name.FullName())
            .RuleFor(
                e => e.DateOfBirth,
                p => p.Date.BetweenDateOnly(
                    DateOnly.FromDateTime(DateTime.Today.AddYears(-42)),
                    DateOnly.FromDateTime(DateTime.Today.AddYears(-21))));

        var employees = employeesFaker.Generate(count: 8);

        var hrManager = employees[0];
        var hrManagerResult = await employeesService.CreateAsync(
            GenerateEmailFromFullName(hrManager.FullName),
            "Password1!",
            hrManager.FullName,
            hrManager.DateOfBirth,
            position: "HR Manager",
            department: "HR",
            managerId: null,
            salary: 10_000,
            BusinessConstants.HrManagerRole,
            createdById: default!);

        if (hrManagerResult.IsError)
        {
            throw new Exception($"Error creating HR manager: {hrManagerResult.ErrorMessage}");
        }

        hrManager.Id = hrManagerResult.Data!;

        this.logger.LogInformation("Created {Position} employee: {Email}", hrManager.Position, hrManager.Email);

        var teamLead = employees[1];
        var teamLeadResult = await employeesService.CreateAsync(
            GenerateEmailFromFullName(teamLead.FullName),
            "Password1!",
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

        this.logger.LogInformation("Created {Position} employee: {Email}", teamLead.Position, teamLead.Email);

        for (var i = 3; i < employees.Count; i++)
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
            var employeeResult = await employeesService.CreateAsync(
                GenerateEmailFromFullName(employee.FullName),
                "Password1!",
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

            this.logger.LogInformation("Created {Position} employee: {Email}", employee.Position, employee.Email);
        }

        return employees;
    }
}
