namespace HrAspire.DataSeeder;

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
            await this.MigrateAndSeedEmployeesDbAsync(stoppingToken);

            await this.MigrateAndSeedSalariesDbAsync(stoppingToken);

            await this.MigrateAndSeedVacationsDbAsync(stoppingToken);
        }

        this.hostApplicationLifetime.StopApplication();
    }

    private async Task MigrateAndSeedEmployeesDbAsync(CancellationToken cancellationToken)
    {
        using var scope = this.scopeFactory.CreateScope();
        using var dbContext = scope.ServiceProvider.GetRequiredService<EmployeesDbContext>();

        this.logger.LogInformation("Migrating employees DB...");
        await dbContext.Database.MigrateAsync(cancellationToken);
        this.logger.LogInformation("Migrated employees DB.");

        var employeesDbSeeder = scope.ServiceProvider.GetRequiredService<EmployeesDbSeeder>();

        await employeesDbSeeder.SeedRolesAsync();

        var employees = await employeesDbSeeder.SeedEmployeesAsync();
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
}
