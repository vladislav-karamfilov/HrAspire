namespace HrAspire.Business.Common.Services;

using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class ProcessOutboxMessagesBackgroundService : BackgroundService
{
    private static readonly TimeSpan TimeToWaitBeforeNextFetchAfterNoMessagesProcessed = TimeSpan.FromSeconds(5);

    private readonly IServiceProvider serviceProvider;
    private readonly ILogger<ProcessOutboxMessagesBackgroundService> logger;

    public ProcessOutboxMessagesBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<ProcessOutboxMessagesBackgroundService> logger)
    {
        this.serviceProvider = serviceProvider;
        this.logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken) => ProcessOutboxMessagesAsync(stoppingToken);

    private async Task ProcessOutboxMessagesAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var processedMessages = 0;

            using (var scope = serviceProvider.CreateScope())
            {
                try
                {
                    var outboxMessagesService = scope.ServiceProvider.GetRequiredService<IOutboxMessagesService>();
                    processedMessages = await outboxMessagesService.ProcessMessagesAsync(cancellationToken);
                }
                catch (TaskCanceledException tce) when (tce.CancellationToken == cancellationToken)
                {
                    // Our cancellation token has been triggered => ignore and stop
                    return;
                }
                catch (Exception e)
                {
                    logger.LogError($"{nameof(this.ProcessOutboxMessagesAsync)}() failed with ex: {e}");
                }
            }

            if (processedMessages == 0)
            {
                await Task.Delay(TimeToWaitBeforeNextFetchAfterNoMessagesProcessed, cancellationToken);
            }
        }
    }
}
