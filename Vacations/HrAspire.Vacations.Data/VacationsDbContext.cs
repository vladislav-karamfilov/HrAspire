namespace HrAspire.Vacations.Data;

using HrAspire.Data.Common;
using HrAspire.Data.Common.Models;
using HrAspire.Vacations.Data.Models;

using Microsoft.EntityFrameworkCore;

public class VacationsDbContext : DbContext
{
    public VacationsDbContext(DbContextOptions<VacationsDbContext> options)
        : base(options)
    {
    }

    public DbSet<VacationRequest> VacationRequests { get; set; }

    public DbSet<OutboxMessage> OutboxMessages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<VacationRequest>().HasIndex(r => r.EmployeeId);

        modelBuilder.Entity<OutboxMessage>().HasIndex(m => m.IsProcessed);

        modelBuilder
            .SetUtcKindToDateTimeProperties()
            .SetGlobalIsDeletedQueryFilter()
            .DisableForeignKeyCascadeDelete();
    }
}
