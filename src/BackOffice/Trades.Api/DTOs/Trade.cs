namespace Trades.Api.DTOs
{
    public record CreateTradeRequest(string AccountId, string Symbol, int Quantity, decimal Price);

    public class Trade
    {
        public Guid Id { get; set; }
        public string AccountId { get; set; } = default!;
        public string Symbol { get; set; } = default!;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public DateTime CreatedAtUtc { get; set; }
    }
}
