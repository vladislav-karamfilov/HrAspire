namespace HrAspire.Data.Common.Models;

public class OutboxMessage
{
    public int Id { get; set; }

    public string Type { get; set; } = default!;

    public string Payload { get; set; } = default!;

    public DateTime CreatedOn { get; set; }

    public bool IsProcessed { get; set; }

    public DateTime? ProcessedOn { get; set; }

    public string? ProcessedResult { get; set; }
}
