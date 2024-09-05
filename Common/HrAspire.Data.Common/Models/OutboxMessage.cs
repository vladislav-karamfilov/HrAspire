namespace HrAspire.Data.Common.Models;

public class OutboxMessage
{
    public int Id { get; set; }

    public string EventType { get; set; } = default!;

    public string EventData { get; set; } = default!;

    public DateTime CreatedOn { get; set; }

    public bool IsProcessed { get; set; }

    public string? ProcessedResult { get; set; }

    public DateTime? ProcessedOn { get; set; }
}
