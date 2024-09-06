using HrAspire.Employees.Business.Documents;
using HrAspire.Employees.Business.Employees;
using HrAspire.Employees.Data;
using HrAspire.Employees.Data.Models;
using HrAspire.Employees.Web.Services;
using HrAspire.Web.Common;

using MassTransit;

using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc();

builder.AddServiceDefaults();

builder.AddNpgsqlDbContext<EmployeesDbContext>("employees-db");

builder.AddAzureBlobClient("blobs");

builder.AddRedisClient("cache");

builder.Services.AddMassTransit(x =>
{
    x.SetKebabCaseEndpointNameFormatter();

    x.AddConsumers(typeof(Program).Assembly);

    x.UsingRabbitMq((context, configurator) =>
    {
        configurator.UseMessageRetry(
            retry => retry.Incremental(
                retryLimit: 5,
                initialInterval: TimeSpan.FromMilliseconds(500),
                intervalIncrement: TimeSpan.FromMilliseconds(500)));

        var configuration = context.GetRequiredService<IConfiguration>();
        var host = configuration.GetConnectionString("messaging");
        configurator.Host(host);
        configurator.ConfigureEndpoints(context);
    });
});

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
