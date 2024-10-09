namespace HrAspire.Vacations.Business.OutboxMessages;

using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using HrAspire.Business.Common.Events;
using HrAspire.Business.Common.Services;
using HrAspire.Data.Common.Models;
using HrAspire.Vacations.Data;

using MassTransit;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public class OutboxMessagesService : IOutboxMessagesService
{
    private readonly VacationsDbContext dbContext;
    private readonly IPublishEndpoint publishEndpoint;
    private readonly TimeProvider timeProvider;
    private readonly ILogger<OutboxMessagesService> logger;

    public OutboxMessagesService(
        VacationsDbContext dbContext,
        IPublishEndpoint publishEndpoint,
        TimeProvider timeProvider,
        ILogger<OutboxMessagesService> logger)
    {
        this.dbContext = dbContext;
        this.publishEndpoint = publishEndpoint;
        this.timeProvider = timeProvider;
        this.logger = logger;
    }

    public async Task<int> ProcessMessagesAsync(CancellationToken cancellationToken)
    {
        const int MaxMessagesToProcess = 20;

        var processedMessages = 0;

        var messagesToProcess = await this.dbContext.OutboxMessages
            .Where(m => !m.IsProcessed)
            .OrderBy(m => m.Id)
            .Take(MaxMessagesToProcess)
            .ToListAsync(cancellationToken);

        foreach (var message in messagesToProcess)
        {
            try
            {
                await this.ProcessMessageAsync(message, cancellationToken);

                await this.dbContext.SaveChangesAsync(cancellationToken);

                processedMessages++;
            }
            catch (TaskCanceledException tce) when (tce.CancellationToken == cancellationToken)
            {
                // Our cancellation token has been triggered => ignore and stop
                break;
            }
            catch (Exception ex)
            {
                this.logger.LogError("Error processing message {MessageId}: {Exception}", message.Id, ex);
            }
        }

        return processedMessages;
    }

    private async Task ProcessMessageAsync(OutboxMessage message, CancellationToken cancellationToken)
    {
        object? payloadObject = null;
        string? errorMessage;
        if (message.Type == typeof(PaidVacationRequestApprovedEvent).FullName)
        {
            (payloadObject, errorMessage) = this.TryDeserializePaidVacationRequestApprovedEvent(message);
        }
        else
        {
            errorMessage = "Unknown message type";
        }

        if (string.IsNullOrEmpty(errorMessage))
        {
            await this.publishEndpoint.Publish(payloadObject!, cancellationToken);
        }

        message.IsProcessed = true;
        message.ProcessedOn = this.timeProvider.GetUtcNow().UtcDateTime;
        message.ProcessingError = errorMessage;
    }

    private (PaidVacationRequestApprovedEvent? @Event, string? ErrorMessage) TryDeserializePaidVacationRequestApprovedEvent(
        OutboxMessage message)
    {
        PaidVacationRequestApprovedEvent? @event = null;
        string? errorMessage = null;
        try
        {
            @event = JsonSerializer.Deserialize<PaidVacationRequestApprovedEvent>(message.Payload);
            if (@event is null)
            {
                errorMessage = "Deserialized payload object is null";
            }
        }
        catch (JsonException ex)
        {
            this.logger.LogError(
                "Error deserializing payload of message {MessageId} to {MessageType}: {Exception}",
                message.Id,
                message.Type,
                ex);

            errorMessage = $"Error deserializing payload: {ex}";
        }

        return (@event, errorMessage);
    }
}
