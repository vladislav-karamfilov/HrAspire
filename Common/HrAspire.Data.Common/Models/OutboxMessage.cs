namespace HrAspire.Data.Common.Models;

public class OutboxMessage
{
    public int Id { get; set; }

    public string Type { get; set; } = default!;

    public string Payload { get; set; } = default!;

    public DateTime CreatedOn { get; set; }

    public bool IsSent { get; set; }

    public DateTime? SentOn { get; set; }
}
