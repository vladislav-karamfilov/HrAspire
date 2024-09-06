namespace HrAspire.Salaries.Business.OutboxMessages;

public interface IOutboxMessagesService
{
    Task<int> ProcessMessagesAsync(CancellationToken cancellationToken);
}
