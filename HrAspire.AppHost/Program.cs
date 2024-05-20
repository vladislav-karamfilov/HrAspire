var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.HrAspire_ApiService>("apiservice");

builder.AddProject<Projects.HrAspire_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService);

builder.Build().Run();
