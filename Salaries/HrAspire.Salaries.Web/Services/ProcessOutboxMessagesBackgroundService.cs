namespace HrAspire.Salaries.Web.Services;

using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using HrAspire.Business.Common.Events;
using HrAspire.Data.Common.Models;
using HrAspire.Salaries.Data;

using MassTransit;

using Microsoft.EntityFrameworkCore;

public class ProcessOutboxMessagesBackgroundService : BackgroundService
{
    private static readonly TimeSpan TimeToWaitBeforeNextFetchAfterNoMessagesSent = TimeSpan.FromSeconds(5);

    private readonly IBus bus;
    private readonly IServiceProvider serviceProvider;
    private readonly TimeProvider timeProvider;
    private readonly ILogger<ProcessOutboxMessagesBackgroundService> logger;

    public ProcessOutboxMessagesBackgroundService(
        IBus bus,
        IServiceProvider serviceProvider,
        TimeProvider timeProvider,
        ILogger<ProcessOutboxMessagesBackgroundService> logger)
    {
        this.bus = bus;
        this.serviceProvider = serviceProvider;
        this.timeProvider = timeProvider;
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
                    var dbContext = scope.ServiceProvider.GetRequiredService<SalariesDbContext>();

                    var messagesToSend = await dbContext.OutboxMessages
                        .Where(m => !m.IsProcessed)
                        .OrderBy(m => m.Id)
                        .ToListAsync(cancellationToken);

                    foreach (var message in messagesToSend)
                    {
                        try
                        {
                            await this.ProcessOutboxMessageAsync(message, cancellationToken);

                            await dbContext.SaveChangesAsync(cancellationToken);

                            processedMessages++;
                        }
                        catch (TaskCanceledException tce) when (tce.CancellationToken == cancellationToken)
                        {
                            // Our cancellation token has been triggered => ignore and stop
                            return;
                        }
                        catch (Exception ex)
                        {
                            this.logger.LogError("Error processing message {messageId}: {exception}", message.Id, ex);
                        }
                    }
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
                await Task.Delay(TimeToWaitBeforeNextFetchAfterNoMessagesSent, cancellationToken);
            }
        }
    }

    private async Task ProcessOutboxMessageAsync(OutboxMessage message, CancellationToken cancellationToken)
    {
        object? payloadObject = null;
        string? errorMessage = null;
        if (message.Type == nameof(SalaryRequestApprovedEvent))
        {
            try
            {
                payloadObject = JsonSerializer.Deserialize<SalaryRequestApprovedEvent>(message.Payload);
                if (payloadObject is null)
                {
                    errorMessage = "Deserialized payload object is null";
                }
            }
            catch (JsonException ex)
            {
                this.logger.LogError(
                    "Error deserializing payload of message {messageId} to {messageType}: {exception}",
                    message.Id,
                    message.Type,
                    ex);

                errorMessage = $"Error deserializing payload to {message.Type}: {ex}";
            }
        }
        else
        {
            errorMessage = "Unknown message type.";
        }

        if (string.IsNullOrEmpty(errorMessage))
        {
            await this.bus.Publish(payloadObject!, cancellationToken);
        }

        message.IsProcessed = true;
        message.ProcessedOn = this.timeProvider.GetUtcNow().UtcDateTime;
        message.ProcessedResult = errorMessage;
    }
}
