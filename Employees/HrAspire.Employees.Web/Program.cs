using HrAspire.Employees.Business.Employees;
using HrAspire.Employees.Data;
using HrAspire.Employees.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc();

builder.AddServiceDefaults();

builder.AddNpgsqlDbContext<EmployeesDbContext>("employees-db");

builder.Services.AddScoped<IEmployeesService, EmployeesService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<EmployeesGrpcService>();

app.MapGet(
    "/",
    () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
