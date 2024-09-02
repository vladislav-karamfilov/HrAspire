using HrAspire.DataSeeder;
using HrAspire.Employees.Data;
using HrAspire.Salaries.Data;

using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

builder.AddNpgsqlDbContext<EmployeesDbContext>(
    "employees-db",
    configureDbContextOptions: options => options.UseNpgsql(b => b.MigrationsAssembly(typeof(Program).Assembly.FullName)));

builder.AddNpgsqlDbContext<SalariesDbContext>(
    "salaries-db",
    configureDbContextOptions: options => options.UseNpgsql(b => b.MigrationsAssembly(typeof(Program).Assembly.FullName)));

builder.Services.AddHostedService<DataSeederWorker>();

var host = builder.Build();
host.Run();
