using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Trades.Api.Database;
using Trades.Api.DTOs;

namespace Trades.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class TradesController : ControllerBase
    {
        private readonly TradesDbContext _db;

        public TradesController(TradesDbContext db)
        {
            _db = db;
        }

        [HttpPost]
        public async Task<IActionResult> CreateTrade([FromBody] CreateTradeRequest request, CancellationToken ct)
        {
            var trade = new Trade
            {
                Id = Guid.NewGuid(),
                AccountId = request.AccountId,
                Symbol = request.Symbol,
                Quantity = request.Quantity,
                Price = request.Price,
                CreatedAtUtc = DateTime.UtcNow
            };

            var tradeCreatedEvent = new
            {
                EventId = Guid.NewGuid(),
                TradeId = trade.Id,
                trade.AccountId,
                trade.Symbol,
                trade.Quantity,
                trade.Price,
                trade.CreatedAtUtc
            };

            try
            {
                using var tx = await _db.Database.BeginTransactionAsync(ct);

                _db.Trades.Add(trade);

                _db.OutboxMessages.Add(new OutboxMessage
                {
                    Id = tradeCreatedEvent.EventId,
                    EventType = "TradeCreated",
                    Payload = JsonSerializer.Serialize(tradeCreatedEvent),
                    CreatedAtUtc = DateTime.UtcNow,
                    Processed = false
                });

                await _db.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);

                return Accepted(new
                {
                    trade.Id
                });
            }
            catch (Exception ex)
            {
                throw;
            }

            
        }
    }
}
