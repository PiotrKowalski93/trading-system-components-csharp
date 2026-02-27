using Microsoft.EntityFrameworkCore;
using Trades.Api.DTOs;

namespace Trades.Api.Database
{
    public class TradesDbContext : DbContext
    {
        public DbSet<OutboxMessage> OutboxMessages { get; set; }
        public DbSet<Trade> Trades { get; set; }

        public TradesDbContext(DbContextOptions<TradesDbContext> options)
            : base(options)
        {
        }
    }
}
