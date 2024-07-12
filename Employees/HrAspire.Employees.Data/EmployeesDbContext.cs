namespace HrAspire.Employees.Data;

using HrAspire.Employees.Data.Models;

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

public class EmployeesDbContext : IdentityDbContext<Employee>
{
    public EmployeesDbContext(DbContextOptions<EmployeesDbContext> options)
        : base(options)
    {
    }

    public DbSet<Employee> Employees { get; set; }

    public DbSet<Document> Documents { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Employee>().ToTable("Employees");
        builder.Entity<Employee>().HasMany(e => e.Documents).WithOne(d => d.Employee);
        builder.Entity<Employee>().HasMany(e => e.CreatedDocuments).WithOne(d => d.CreatedBy);
        builder.Entity<Employee>().HasOne(e => e.CreatedBy).WithMany(e => e.CreatedEmployees);

        // Consider all DateTime props in DB as values in UTC timezone
        var specifyUtcDateTimeKindValueConverter = new ValueConverter<DateTime, DateTime>(
            v => v,
            v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

        var properties = builder.Model
            .GetEntityTypes()
            .SelectMany(e => e.GetProperties().Where(p => p.ClrType == typeof(DateTime) || p.ClrType == typeof(DateTime?)));

        foreach (var property in properties)
        {
            property.SetValueConverter(specifyUtcDateTimeKindValueConverter);
        }

        // Disable cascade delete
        var foreignKeys = builder.Model
            .GetEntityTypes()
            .SelectMany(e => e.GetForeignKeys().Where(f => f.DeleteBehavior == DeleteBehavior.Cascade));

        foreach (var foreignKey in foreignKeys)
        {
            foreignKey.DeleteBehavior = DeleteBehavior.Restrict;
        }
    }
}
