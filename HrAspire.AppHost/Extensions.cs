namespace HrAspire.AppHost;

internal static class Extensions
{
    public static IResourceBuilder<GarnetResource> WithRedisInsight(
        this IResourceBuilder<GarnetResource> garnetBuilder,
        ContainerLifetime containerLifetime,
        string? containerName = null,
        string? containerTag = null)
    {
        ArgumentNullException.ThrowIfNull(garnetBuilder);

        var garnetResource = garnetBuilder.Resource;

        containerName ??= $"{garnetResource.Name}-insight";
        containerTag ??= "2.68";

        garnetBuilder.ApplicationBuilder.AddContainer(containerName, image: "redis/redisinsight", containerTag)
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
}
