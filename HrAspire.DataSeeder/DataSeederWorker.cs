namespace HrAspire.DataSeeder;

using System.Threading;
using System.Threading.Tasks;

using HrAspire.Business.Common;
using HrAspire.Employees.Data;
using HrAspire.Salaries.Data;

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

        // TODO: extract to separate method and seed more data - employees, documents(?)
        if (!dbContext.Roles.Any())
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
}
