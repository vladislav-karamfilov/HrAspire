using HrAspire.DataSeeder;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddHostedService<DataSeederWorker>();

var host = builder.Build();
host.Run();
