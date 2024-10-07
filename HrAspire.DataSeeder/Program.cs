using HrAspire.DataSeeder;
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
    configureDbContextOptions: options => options
        .UseNpgsql(b => b.MigrationsAssembly(typeof(Program).Assembly.FullName))
        .EnableDetailedErrors(true));

builder.AddNpgsqlDbContext<SalariesDbContext>(
    ResourceNames.SalariesDb,
    configureDbContextOptions: options => options
        .UseNpgsql(b => b.MigrationsAssembly(typeof(Program).Assembly.FullName))
        .EnableDetailedErrors(true));

builder.AddNpgsqlDbContext<VacationsDbContext>(
    ResourceNames.VacationsDb,
    configureDbContextOptions: options => options
        .UseNpgsql(b => b.MigrationsAssembly(typeof(Program).Assembly.FullName))
        .EnableDetailedErrors(true));

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

builder.Services.AddHostedService<DataSeederWorker>();

var host = builder.Build();
host.Run();
