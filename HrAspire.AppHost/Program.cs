﻿using HrAspire.AppHost;
using HrAspire.ServiceDefaults;

var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder
    .AddPostgres(ResourceNames.Postgres)
    .WithDataVolume("HrAspire-db-data")
    .WithLifetime(ContainerLifetime.Persistent);

postgres.WithPgAdmin(c => c.WithLifetime(ContainerLifetime.Persistent).WithParentRelationship(postgres));

var employeesDb = postgres.AddDatabase(ResourceNames.EmployeesDb, "employees").WithApplyDbMigrationsCommand();
var salariesDb = postgres.AddDatabase(ResourceNames.SalariesDb, "salaries").WithApplyDbMigrationsCommand();
var vacationsDb = postgres.AddDatabase(ResourceNames.VacationsDb, "vacations").WithApplyDbMigrationsCommand();

var azureStorage = builder
    .AddAzureStorage(ResourceNames.AzureStorage)
    .RunAsEmulator(c => c
        .WithDataVolume("HrAspire-blob-data")
        .WithBlobPort(22192)
        .WithLifetime(ContainerLifetime.Persistent)
        .WithImageTag("3.33.0"));

var blobs = azureStorage.AddBlobs(ResourceNames.Blobs);

var cache = builder
    .AddGarnet(ResourceNames.Cache)
    .WithDataVolume("HrAspire-cache-data")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithRedisInsight(ContainerLifetime.Persistent)
    .WithImageTag("1.0.81");

var messagingUserParameter = builder.AddParameter("messaging-user", secret: true);
var messagingPasswordParameter = builder.AddParameter("messaging-password", secret: true);

var messaging = builder
    .AddRabbitMQ(ResourceNames.Messaging, messagingUserParameter, messagingPasswordParameter)
    .WithDataVolume("HrAspire-messaging-data")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithManagementPlugin();

messagingUserParameter = messagingUserParameter.WithParentRelationship(messaging);
messagingPasswordParameter = messagingPasswordParameter.WithParentRelationship(messaging);

var employeesService = builder
    .AddProject<Projects.HrAspire_Employees_Web>(ResourceNames.EmployeesService)
    .WithReference(employeesDb)
    .WithReference(blobs)
    .WithReference(cache)
    .WithReference(messaging)
    .WaitFor(employeesDb)
    .WaitFor(blobs)
    .WaitFor(cache)
    .WaitFor(messaging);

var salariesService = builder
    .AddProject<Projects.HrAspire_Salaries_Web>(ResourceNames.SalariesService)
    .WithReference(salariesDb)
    .WithReference(cache)
    .WithReference(messaging)
    .WaitFor(salariesDb)
    .WaitFor(cache)
    .WaitFor(messaging);

var vacationsService = builder
    .AddProject<Projects.HrAspire_Vacations_Web>(ResourceNames.VacationsService)
    .WithReference(vacationsDb)
    .WithReference(cache)
    .WithReference(messaging)
    .WaitFor(vacationsDb)
    .WaitFor(cache)
    .WaitFor(messaging);

var apiGateway = builder
    .AddProject<Projects.HrAspire_Web_ApiGateway>(ResourceNames.ApiGateway)
    .WithReference(employeesDb)
    .WithReference(employeesService)
    .WithReference(salariesService)
    .WithReference(vacationsService)
    .WaitFor(employeesService)
    .WaitFor(salariesService)
    .WaitFor(vacationsService)
    .WithExternalHttpEndpoints();

var apiGatewayEndpoint = apiGateway.GetEndpoint("https");

var webFrontEnd = builder
    .AddProject<Projects.HrAspire_Web>(ResourceNames.WebFrontEnd)
    .WithReference(apiGateway)
    .WaitFor(apiGateway)
    .WithExternalHttpEndpoints()
    .WithEnvironment(EnvironmentVariableNames.ApiGatewayUrl, apiGatewayEndpoint);

var webFrontEndEndpoint = webFrontEnd.GetEndpoint("https");

apiGateway = apiGateway.WithEnvironment(EnvironmentVariableNames.WebFrontEndUrl, webFrontEndEndpoint);

if (builder.ExecutionContext.IsRunMode)
{
    builder
        .AddProject<Projects.HrAspire_DataSeeder>(ResourceNames.DataSeeder)
        .WithReference(employeesDb)
        .WithReference(salariesDb)
        .WithReference(vacationsDb)
        .WithReference(blobs)
        .WithReference(cache)
        .WaitFor(employeesDb)
        .WaitFor(salariesDb)
        .WaitFor(vacationsDb)
        .WaitFor(blobs)
        .WaitFor(cache);
}

builder.Build().Run();
