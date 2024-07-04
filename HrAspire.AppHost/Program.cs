var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder
    .AddPostgres("postgres")
    .WithDataVolume("db-data")
    .WithPgAdmin();

var employeesDb = postgres.AddDatabase("employees-db", "employees");
var salariesDb = postgres.AddDatabase("salaries-db", "salaries");
var vacationsDb = postgres.AddDatabase("vacations-db", "vacations");

var azureStorage = builder
    .AddAzureStorage("azure-storage")
    .RunAsEmulator(c => c.WithDataVolume("blob-data"));

var blobs = azureStorage.AddBlobs("blobs");

var apiGateway = builder
    .AddProject<Projects.HrAspire_Web_ApiGateway>("api-gateway")
    .WithReference(employeesDb)
    .WithExternalHttpEndpoints();

builder
    .AddProject<Projects.HrAspire_Web>("web-front-end")
    .WithReference(apiGateway)
    .WithExternalHttpEndpoints();

builder.Build().Run();
