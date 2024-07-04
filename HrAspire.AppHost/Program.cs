var builder = DistributedApplication.CreateBuilder(args);

var db = builder
    .AddPostgres("db")
    .WithDataVolume("db-data")
    .AddDatabase("hr-aspire");

var blobs = builder
    .AddAzureStorage("storage")
    .RunAsEmulator(c => c.WithDataVolume("blob-data"))
    .AddBlobs("blobs");

//var apiService = builder
//    .AddProject<Projects.HrAspire_ApiService>("apiservice")
//    .WithReference(db)
//    .WithReference(blobs)
//    .WithReplicas(2);

builder
    .AddProject<Projects.HrAspire_Web>("webfrontend")
    .WithExternalHttpEndpoints();

builder.Build().Run();
