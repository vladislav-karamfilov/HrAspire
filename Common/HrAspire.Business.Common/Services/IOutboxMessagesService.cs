namespace HrAspire.Business.Common.Services;

public interface IOutboxMessagesService
{
    Task<int> ProcessMessagesAsync(CancellationToken cancellationToken);
}
