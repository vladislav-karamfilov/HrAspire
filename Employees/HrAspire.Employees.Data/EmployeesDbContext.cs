namespace HrAspire.Employees.Data;

using HrAspire.Data.Common;
using HrAspire.Employees.Data.Models;

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

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

        var employeeBuilder = builder.Entity<Employee>();

        employeeBuilder.ToTable("Employees");
        employeeBuilder.HasMany(e => e.Documents).WithOne(d => d.Employee);
        employeeBuilder.HasMany(e => e.CreatedDocuments).WithOne(d => d.CreatedBy);
        employeeBuilder.HasOne(e => e.CreatedBy).WithMany(e => e.CreatedEmployees);
        employeeBuilder.HasMany(e => e.Roles).WithOne().HasForeignKey(ur => ur.UserId);

        builder
            .SetUtcKindToDateTimeProperties()
            .SetGlobalIsDeletedQueryFilter()
            .DisableForeignKeyCascadeDelete();
    }
}
