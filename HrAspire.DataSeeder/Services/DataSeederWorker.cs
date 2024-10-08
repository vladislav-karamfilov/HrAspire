namespace HrAspire.DataSeeder.Services;

using System.Threading;
using System.Threading.Tasks;

using HrAspire.Employees.Data;
using HrAspire.Salaries.Data;
using HrAspire.Vacations.Data;

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
            var (hrManagerId, managerId, employeeIds) = await this.MigrateAndSeedEmployeesDbAsync(stoppingToken);
            if (hrManagerId is not null && managerId is not null)
            {
                await this.MigrateAndSeedSalariesDbAsync(managerId, employeeIds, stoppingToken);

                await this.MigrateAndSeedVacationsDbAsync(employeeIds.Concat([hrManagerId, managerId]), stoppingToken);
            }
        }

        this.hostApplicationLifetime.StopApplication();
    }

    private async Task<(string? HrManagerId, string? ManagerId, string[] EmployeeIds)> MigrateAndSeedEmployeesDbAsync(
        CancellationToken cancellationToken)
    {
        using var scope = this.scopeFactory.CreateScope();
        using var dbContext = scope.ServiceProvider.GetRequiredService<EmployeesDbContext>();

        this.logger.LogInformation("Migrating employees DB...");
        await dbContext.Database.MigrateAsync(cancellationToken);
        this.logger.LogInformation("Migrated employees DB.");

        var employeesDbSeeder = scope.ServiceProvider.GetRequiredService<EmployeesDbSeeder>();

        await employeesDbSeeder.SeedRolesAsync();

        var (hrManagerId, managerId, employeeIds) = await employeesDbSeeder.SeedEmployeesAsync();
        if (hrManagerId is not null)
        {
            await employeesDbSeeder.SeedDocumentsAsync(hrManagerId, employeeIds.Concat([hrManagerId, managerId!]));
        }

        return (hrManagerId, managerId, employeeIds);
    }

    private async Task MigrateAndSeedSalariesDbAsync(string managerId, string[] employeeIds, CancellationToken cancellationToken)
    {
        using var scope = this.scopeFactory.CreateScope();
        using var dbContext = scope.ServiceProvider.GetRequiredService<SalariesDbContext>();

        this.logger.LogInformation("Migrating salaries DB...");
        await dbContext.Database.MigrateAsync(cancellationToken);
        this.logger.LogInformation("Migrated salaries DB.");

        var salariesDbSeeder = scope.ServiceProvider.GetRequiredService<SalariesDbSeeder>();

        await salariesDbSeeder.SeedSalaryRequestsAsync(managerId, employeeIds);
    }

    private async Task MigrateAndSeedVacationsDbAsync(IEnumerable<string> employeeIds, CancellationToken cancellationToken)
    {
        using var scope = this.scopeFactory.CreateScope();
        using var dbContext = scope.ServiceProvider.GetRequiredService<VacationsDbContext>();

        this.logger.LogInformation("Migrating vacations DB...");
        await dbContext.Database.MigrateAsync(cancellationToken);
        this.logger.LogInformation("Migrated vacations DB.");

        var vacationsDbSeeder = scope.ServiceProvider.GetRequiredService<VacationsDbSeeder>();

        await vacationsDbSeeder.SeedVacationRequestsAsync(employeeIds);
    }
}
