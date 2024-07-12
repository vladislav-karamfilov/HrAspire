using HrAspire.AppHost;

var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder
    .AddPostgres(
        "postgres",
        // workaround for https://github.com/dotnet/aspire/issues/1151
        password: builder.CreateStablePassword("postgres-password", special: false))
    .WithDataVolume("db-data")
    .WithPgAdmin();

var employeesDb = postgres.AddDatabase("employees-db", "employees");
var salariesDb = postgres.AddDatabase("salaries-db", "salaries");
var vacationsDb = postgres.AddDatabase("vacations-db", "vacations");

var azureStorage = builder
    .AddAzureStorage("azure-storage")
    .RunAsEmulator(c => c.WithDataVolume("blob-data"));

var blobs = azureStorage.AddBlobs("blobs");

var employeesService = builder
    .AddProject<Projects.HrAspire_Employees_Web>("employees-service")
    .WithReference(employeesDb)
    .WithReference(blobs);

var apiGateway = builder
    .AddProject<Projects.HrAspire_Web_ApiGateway>("api-gateway")
    .WithReference(employeesDb)
    .WithReference(employeesService)
    .WithExternalHttpEndpoints();

var apiGatewayEndpoint = apiGateway.GetEndpoint("https");

var webFrontEnd = builder
    .AddProject<Projects.HrAspire_Web>("web-front-end")
    .WithReference(apiGateway)
    .WithExternalHttpEndpoints()
    .WithEnvironment("ApiGatewayUrl", apiGatewayEndpoint);

var webFrontEndEndpoint = webFrontEnd.GetEndpoint("https");

apiGateway = apiGateway.WithEnvironment("WebFrontEndUrl", webFrontEndEndpoint);

builder.Build().Run();
