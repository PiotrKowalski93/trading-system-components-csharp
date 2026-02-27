namespace Trades.Api.Database
{
    public class OutboxMessage
    {
        public Guid Id { get; set; }
        public string EventType { get; set; } = default!;
        public string Payload { get; set; } = default!;
        public DateTime CreatedAtUtc { get; set; }
        public bool Processed { get; set; }
    }
}
