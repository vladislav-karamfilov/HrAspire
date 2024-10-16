﻿using HrAspire.ServiceDefaults;

var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres(ResourceNames.Postgres).WithPgAdmin().WithDataVolume("HrAspire-db-data");
var employeesDb = postgres.AddDatabase(ResourceNames.EmployeesDb, "employees");
var salariesDb = postgres.AddDatabase(ResourceNames.SalariesDb, "salaries");
var vacationsDb = postgres.AddDatabase(ResourceNames.VacationsDb, "vacations");

var azureStorage = builder
    .AddAzureStorage(ResourceNames.AzureStorage)
    .RunAsEmulator(c => c.WithDataVolume("HrAspire-blob-data").WithImageTag("3.32.0").WithBlobPort(22192));

var blobs = azureStorage.AddBlobs(ResourceNames.Blobs);

var cache = builder
    .AddGarnet(ResourceNames.Cache)
    //// TODO: .WithDataVolume("cache-data") // workaround for https://github.com/dotnet/aspire/issues/4870
    .WithVolume("HrAspire-cache-data", "/data")
    .WithArgs("--checkpointdir", "/data/checkpoints", "--recover", "--aof", "--aof-commit-freq", "60000");

var messaging = builder
    .AddRabbitMQ(
        ResourceNames.Messaging,
        builder.AddParameter("messaging-user", secret: true),
        builder.AddParameter("messaging-password", secret: true))
    .WithManagementPlugin()
    .WithDataVolume("HrAspire-messaging-data");

var employeesService = builder
    .AddProject<Projects.HrAspire_Employees_Web>(ResourceNames.EmployeesService)
    .WithReference(employeesDb)
    .WithReference(blobs)
    .WithReference(cache)
    .WithReference(messaging);

var salariesService = builder
    .AddProject<Projects.HrAspire_Salaries_Web>(ResourceNames.SalariesService)
    .WithReference(salariesDb)
    .WithReference(cache)
    .WithReference(messaging);

var vacationsService = builder
    .AddProject<Projects.HrAspire_Vacations_Web>(ResourceNames.VacationsService)
    .WithReference(vacationsDb)
    .WithReference(cache)
    .WithReference(messaging);

var apiGateway = builder
    .AddProject<Projects.HrAspire_Web_ApiGateway>(ResourceNames.ApiGateway)
    .WithReference(employeesDb)
    .WithReference(employeesService)
    .WithReference(salariesService)
    .WithReference(vacationsService)
    .WithExternalHttpEndpoints();

var apiGatewayEndpoint = apiGateway.GetEndpoint("https");

var webFrontEnd = builder
    .AddProject<Projects.HrAspire_Web>(ResourceNames.WebFrontEnd)
    .WithReference(apiGateway)
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
        .WithReference(cache);
}

builder.Build().Run();
