namespace HrAspire.Salaries.Web.Services;

using System.Threading;
using System.Threading.Tasks;

using HrAspire.Salaries.Business.OutboxMessages;

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

    protected override Task ExecuteAsync(CancellationToken stoppingToken) => this.ProcessOutboxMessagesAsync(stoppingToken);

    private async Task ProcessOutboxMessagesAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var processedMessages = 0;

            using (var scope = this.serviceProvider.CreateScope())
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
                    this.logger.LogError($"{nameof(this.ProcessOutboxMessagesAsync)}() failed with ex: {e}");
                }
            }

            if (processedMessages == 0)
            {
                await Task.Delay(TimeToWaitBeforeNextFetchAfterNoMessagesProcessed, cancellationToken);
            }
        }
    }
}
