using HrAspire.DataSeeder.Services;
using HrAspire.Employees.Business.Documents;
using HrAspire.Employees.Business.Employees;
using HrAspire.Employees.Data;
using HrAspire.Employees.Data.Models;
using HrAspire.Salaries.Business.SalaryRequests;
using HrAspire.Salaries.Data;
using HrAspire.ServiceDefaults;
using HrAspire.Vacations.Business.VacationRequests;
using HrAspire.Vacations.Data;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

builder.AddNpgsqlDbContext<EmployeesDbContext>(
    ResourceNames.EmployeesDb,
    configureDbContextOptions: options => options.UseNpgsql(b => b.MigrationsAssembly(typeof(Program).Assembly.FullName)));

builder.AddNpgsqlDbContext<SalariesDbContext>(
    ResourceNames.SalariesDb,
    configureDbContextOptions: options => options.UseNpgsql(b => b.MigrationsAssembly(typeof(Program).Assembly.FullName)));

builder.AddNpgsqlDbContext<VacationsDbContext>(
    ResourceNames.VacationsDb,
    configureDbContextOptions: options => options.UseNpgsql(b => b.MigrationsAssembly(typeof(Program).Assembly.FullName)));

builder.AddAzureBlobClient(ResourceNames.Blobs);

builder.AddRedisClient(ResourceNames.Cache);

builder.Services
    .AddIdentityCore<Employee>(options => options.User.RequireUniqueEmail = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<EmployeesDbContext>();

builder.Services.AddScoped<IEmployeesService, EmployeesService>();
builder.Services.AddScoped<IDocumentsService, DocumentsService>();
builder.Services.AddScoped<ISalaryRequestsService, SalaryRequestsService>();
builder.Services.AddScoped<IVacationRequestsService, VacationRequestsService>();

builder.Services.AddScoped<EmployeesDbSeeder>();
builder.Services.AddScoped<SalariesDbSeeder>();

builder.Services.AddHostedService<DataSeederWorker>();

var host = builder.Build();
host.Run();
