﻿namespace HrAspire.Salaries.Data;

using HrAspire.Data.Common;
using HrAspire.Data.Common.Models;
using HrAspire.Salaries.Data.Models;

using Microsoft.EntityFrameworkCore;

public class SalariesDbContext : DbContext
{
    public SalariesDbContext(DbContextOptions<SalariesDbContext> options)
        : base(options)
    {
    }

    public DbSet<SalaryRequest> SalaryRequests { get; set; }

    public DbSet<OutboxMessage> OutboxMessages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var salaryRequestsBuilder = modelBuilder.Entity<SalaryRequest>();

        salaryRequestsBuilder.Property(r => r.NewSalary).HasPrecision(precision: 18, scale: 6);
        salaryRequestsBuilder.HasIndex(r => r.EmployeeId);

        modelBuilder.Entity<OutboxMessage>().HasIndex(m => m.IsProcessed);

        modelBuilder
            .SetUtcKindToDateTimeProperties()
            .SetGlobalIsDeletedQueryFilter()
            .DisableForeignKeyCascadeDelete();
    }
}
