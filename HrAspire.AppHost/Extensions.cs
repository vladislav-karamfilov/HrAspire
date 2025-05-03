namespace HrAspire.AppHost;

using System.Diagnostics;

using Humanizer;

internal static class Extensions
{
    public static IResourceBuilder<GarnetResource> WithRedisInsight(
        this IResourceBuilder<GarnetResource> garnetBuilder,
        ContainerLifetime containerLifetime,
        string? containerName = null,
        string? imageTag = null)
    {
        ArgumentNullException.ThrowIfNull(garnetBuilder);

        var garnetResource = garnetBuilder.Resource;

        containerName ??= $"{garnetResource.Name}-insight";
        imageTag ??= "2.68";

        garnetBuilder.ApplicationBuilder
            .AddContainer(containerName, image: "redis/redisinsight", imageTag)
            .WithImageRegistry("docker.io")
            .WithLifetime(containerLifetime)
            .WithHttpEndpoint(targetPort: 5540, name: "http")
            .WithEnvironment(context =>
            {
                context.EnvironmentVariables.Add($"RI_REDIS_HOST1", garnetResource.Name);
                context.EnvironmentVariables.Add($"RI_REDIS_PORT1", garnetResource.PrimaryEndpoint.TargetPort!.Value);
                context.EnvironmentVariables.Add($"RI_REDIS_ALIAS1", garnetResource.Name);
                if (garnetResource.PasswordParameter is not null)
                {
                    context.EnvironmentVariables.Add($"RI_REDIS_PASSWORD1", garnetResource.PasswordParameter.Value);
                }
            })
            .WithRelationship(garnetResource, "RedisInsight")
            .WithParentRelationship(garnetResource)
            .ExcludeFromManifest();

        return garnetBuilder;
    }

    public static IResourceBuilder<ProjectResource> WithApplyDbMigrationsCommand(
        this IResourceBuilder<ProjectResource> projectBuilder,
        string databaseResourceName)
    {
        ArgumentNullException.ThrowIfNull(projectBuilder);
        ArgumentException.ThrowIfNullOrWhiteSpace(databaseResourceName);

        var endIndex = databaseResourceName.LastIndexOf("-db", StringComparison.Ordinal);
        if (endIndex < 0)
        {
            throw new ArgumentException("Invalid database resource name - it should end with '-db'", nameof(databaseResourceName));
        }

        var databaseResource = projectBuilder.ApplicationBuilder.Resources
            .OfType<PostgresDatabaseResource>()
            .SingleOrDefault(r => r.Name == databaseResourceName);

        if (databaseResource is null)
        {
            throw new ArgumentException(
                "No PostgreSQL resource found with the specified database resource name",
                nameof(databaseResourceName));
        }

        var databaseName = databaseResourceName[..endIndex];

        return projectBuilder.WithCommand(
            $"{databaseName}-db-migrations",
            $"Apply '{databaseName}' DB migrations",
            async context =>
            {
                var dbConnectionString = await databaseResource.ConnectionStringExpression.GetValueAsync(context.CancellationToken);

                var dotnetProcessInfo = new ProcessStartInfo("dotnet")
                {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = Path.GetDirectoryName(new Projects.HrAspire_DataSeeder().ProjectPath),
                    Arguments = $"ef database update --context {databaseName.Pascalize()}DbContext --connection \"{dbConnectionString}\"",
                };

                try
                {
                    using var process = Process.Start(dotnetProcessInfo);
                    if (process is null)
                    {
                        return CommandResults.Failure("Couldn't start 'dotnet-ef' global tool.");
                    }

                    await process.WaitForExitAsync(context.CancellationToken);

                    var error = await process.StandardError.ReadToEndAsync(context.CancellationToken);
                    if (!string.IsNullOrEmpty(error))
                    {
                        return CommandResults.Failure(error);
                    }

                    var output = await process.StandardOutput.ReadToEndAsync(context.CancellationToken);
                    if (output?.TrimEnd().EndsWith("\nDone.", StringComparison.Ordinal) != true)
                    {
                        return CommandResults.Failure(output);
                    }
                }
                catch (Exception ex)
                {
                    return CommandResults.Failure(ex);
                }

                return CommandResults.Success();
            },
            new CommandOptions
            {
                IconName = "DatabaseArrowUp",
                Description = $"Applies pending DB migrations to the '{databaseName}' DB",
            });
    }
}
