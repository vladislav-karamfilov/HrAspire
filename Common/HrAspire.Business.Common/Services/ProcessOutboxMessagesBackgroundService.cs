namespace HrAspire.Business.Common.Services;

using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class ProcessOutboxMessagesBackgroundService : BackgroundService
{
    private static readonly TimeSpan TimeToWaitBeforeNextFetchAfterNoMessagesProcessed = TimeSpan.FromSeconds(5);

    private readonly IServiceScopeFactory serviceScopeFactory;
    private readonly ILogger<ProcessOutboxMessagesBackgroundService> logger;

    public ProcessOutboxMessagesBackgroundService(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<ProcessOutboxMessagesBackgroundService> logger)
    {
        this.serviceScopeFactory = serviceScopeFactory;
        this.logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken) => this.ProcessOutboxMessagesAsync(stoppingToken);

    private async Task ProcessOutboxMessagesAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var processedMessages = 0;

            using (var scope = this.serviceScopeFactory.CreateScope())
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
                    this.logger.LogError("{Method}() failed with ex: {Exception}", nameof(this.ProcessOutboxMessagesAsync), e);
                }
            }

            if (processedMessages == 0)
            {
                await Task.Delay(TimeToWaitBeforeNextFetchAfterNoMessagesProcessed, cancellationToken);
            }
        }
    }
}
