namespace HrAspire.Vacations.Data;

using HrAspire.Data.Common;
using HrAspire.Vacations.Data.Models;

using Microsoft.EntityFrameworkCore;

public class VacationsDbContext : DbContext
{
    public VacationsDbContext(DbContextOptions<VacationsDbContext> options)
        : base(options)
    {
    }

    public DbSet<VacationRequest> VacationRequests { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder
            .SetUtcKindToDateTimeProperties()
            .SetGlobalIsDeletedQueryFilter()
            .DisableForeignKeyCascadeDelete();
    }
}
