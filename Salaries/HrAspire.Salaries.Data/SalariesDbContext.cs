namespace HrAspire.Salaries.Data;

using HrAspire.Data.Common;
using HrAspire.Salaries.Data.Models;

using Microsoft.EntityFrameworkCore;

public class SalariesDbContext : DbContext
{
    public SalariesDbContext(DbContextOptions<SalariesDbContext> options)
        : base(options)
    {
    }

    public DbSet<SalaryRequest> SalaryRequests { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<SalaryRequest>().Property(r => r.NewSalary).HasPrecision(precision: 18, scale: 6);

        modelBuilder
            .SetUtcKindToDateTimeProperties()
            .SetGlobalIsDeletedQueryFilter()
            .DisableForeignKeyCascadeDelete();
    }
}
