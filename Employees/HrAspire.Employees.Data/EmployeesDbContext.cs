namespace HrAspire.Employees.Data;

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

        builder.Entity<Employee>().ToTable("Employees");
        builder.Entity<Employee>().HasMany(e => e.Documents).WithOne(d => d.Employee);
        builder.Entity<Employee>().HasMany(e => e.CreatedDocuments).WithOne(d => d.CreatedBy);
    }
}
