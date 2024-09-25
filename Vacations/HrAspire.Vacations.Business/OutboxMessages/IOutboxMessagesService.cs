namespace HrAspire.Vacations.Business.OutboxMessages;

public interface IOutboxMessagesService
{
    Task<int> ProcessMessagesAsync(CancellationToken cancellationToken);
}
