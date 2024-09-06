var builder = DistributedApplication.CreateBuilder(args);

// TODO: extract resource names into a constants class in ServiceDefaults
var postgres = builder.AddPostgres("postgres").WithPgAdmin().WithDataVolume("HrAspire-db-data");
var employeesDb = postgres.AddDatabase("employees-db", "employees");
var salariesDb = postgres.AddDatabase("salaries-db", "salaries");
var vacationsDb = postgres.AddDatabase("vacations-db", "vacations");

var azureStorage = builder
    .AddAzureStorage("azure-storage")
    .RunAsEmulator(c => c.WithDataVolume("HrAspire-blob-data"));

var blobs = azureStorage.AddBlobs("blobs");

var cache = builder
    .AddGarnet("cache")
    // TODO: .WithDataVolume("cache-data") // workaround for https://github.com/dotnet/aspire/issues/4870
    .WithVolume("HrAspire-cache-data", "/data")
    .WithArgs("--checkpointdir", "/data/checkpoints", "--recover", "--aof", "--aof-commit-freq", "60000");

var messaging = builder
    .AddRabbitMQ(
        "messaging",
        builder.AddParameter("messaging-user", secret: true),
        builder.AddParameter("messaging-password", secret: true))
    .WithManagementPlugin()
    .WithDataVolume("HrAspire-messaging-data");

var employeesService = builder
    .AddProject<Projects.HrAspire_Employees_Web>("employees-service")
    .WithReference(employeesDb)
    .WithReference(blobs)
    .WithReference(cache)
    .WithReference(messaging);

var salariesService = builder
    .AddProject<Projects.HrAspire_Salaries_Web>("salaries-service")
    .WithReference(salariesDb)
    .WithReference(cache)
    .WithReference(messaging);

var apiGateway = builder
    .AddProject<Projects.HrAspire_Web_ApiGateway>("api-gateway")
    .WithReference(employeesDb)
    .WithReference(employeesService)
    .WithReference(salariesService)
    .WithExternalHttpEndpoints();

var apiGatewayEndpoint = apiGateway.GetEndpoint("https");

var webFrontEnd = builder
    .AddProject<Projects.HrAspire_Web>("web-front-end")
    .WithReference(apiGateway)
    .WithExternalHttpEndpoints()
    .WithEnvironment("ApiGatewayUrl", apiGatewayEndpoint);

var webFrontEndEndpoint = webFrontEnd.GetEndpoint("https");

apiGateway = apiGateway.WithEnvironment("WebFrontEndUrl", webFrontEndEndpoint);

builder
    .AddProject<Projects.HrAspire_DataSeeder>("data-seeder")
    .WithReference(employeesDb)
    .WithReference(salariesDb)
    .WithReference(cache);

builder.Build().Run();
