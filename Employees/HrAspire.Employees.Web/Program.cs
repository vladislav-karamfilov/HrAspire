using HrAspire.Employees.Business.Documents;
using HrAspire.Employees.Business.Employees;
using HrAspire.Employees.Data;
using HrAspire.Employees.Data.Models;
using HrAspire.Employees.Web.Services;
using HrAspire.Web.Common;

using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc();

builder.AddServiceDefaults();

builder.AddNpgsqlDbContext<EmployeesDbContext>("employees-db");

builder.AddAzureBlobClient("blobs");

builder.Services.AddScoped<IEmployeesService, EmployeesService>();
builder.Services.AddScoped<IDocumentsService, DocumentsService>();

builder.Services
    .AddIdentityCore<Employee>(options =>
    {
        options.User.RequireUniqueEmail = true;
        options.Password.RequiredLength = AccountConstants.PasswordMinLength;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<EmployeesDbContext>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<EmployeesGrpcService>();
app.MapGrpcService<DocumentsGrpcService>();

app.MapGet(
    "/",
    () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.MapDefaultEndpoints();

app.Run();
