using HrAspire.Business.Common.Services;
using HrAspire.Salaries.Business.OutboxMessages;
using HrAspire.Salaries.Business.SalaryRequests;
using HrAspire.Salaries.Data;
using HrAspire.Salaries.Web.Services;
using HrAspire.ServiceDefaults;

using MassTransit;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc();

builder.AddServiceDefaults();

builder.AddNpgsqlDbContext<SalariesDbContext>(ResourceNames.SalariesDb);

builder.AddRedisClient(ResourceNames.Cache);

builder.Services.AddMassTransit(x =>
{
    x.SetKebabCaseEndpointNameFormatter();

    x.UsingRabbitMq((context, configurator) =>
    {
        var configuration = context.GetRequiredService<IConfiguration>();
        var host = configuration.GetConnectionString(ResourceNames.Messaging);
        configurator.Host(host);
        configurator.ConfigureEndpoints(context);
    });
});

builder.Services.AddScoped<ISalaryRequestsService, SalaryRequestsService>();
builder.Services.AddScoped<IOutboxMessagesService, OutboxMessagesService>();

builder.Services.AddHostedService<ProcessOutboxMessagesBackgroundService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<SalaryRequestsGrpcService>();

app.MapGet(
    "/",
    () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.MapDefaultEndpoints();

app.Run();
