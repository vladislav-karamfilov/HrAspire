using HrAspire.Business.Common.Services;
using HrAspire.ServiceDefaults;
using HrAspire.Vacations.Business.OutboxMessages;
using HrAspire.Vacations.Business.VacationRequests;
using HrAspire.Vacations.Data;
using HrAspire.Vacations.Web.Services;

using MassTransit;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc();

builder.AddServiceDefaults();

builder.AddNpgsqlDbContext<VacationsDbContext>(ResourceNames.VacationsDb);

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

builder.Services.AddScoped<IVacationRequestsService, VacationRequestsService>();
builder.Services.AddScoped<IOutboxMessagesService, OutboxMessagesService>();

builder.Services.AddHostedService<ProcessOutboxMessagesBackgroundService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<VacationRequestsGrpcService>();

app.MapGet(
    "/", 
    () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.MapDefaultEndpoints();

app.Run();
