using HrAspire.DataSeeder;
using HrAspire.Employees.Data;
using HrAspire.Salaries.Data;
using HrAspire.ServiceDefaults;

using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

builder.AddNpgsqlDbContext<EmployeesDbContext>(
    ResourceNames.EmployeesDb,
    configureDbContextOptions: options => options.UseNpgsql(b => b.MigrationsAssembly(typeof(Program).Assembly.FullName)));

builder.AddNpgsqlDbContext<SalariesDbContext>(
    ResourceNames.SalariesDb,
    configureDbContextOptions: options => options.UseNpgsql(b => b.MigrationsAssembly(typeof(Program).Assembly.FullName)));

builder.Services.AddHostedService<DataSeederWorker>();

var host = builder.Build();
host.Run();
