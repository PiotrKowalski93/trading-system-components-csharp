
using Microsoft.EntityFrameworkCore;
using Trades.Api.Database;

namespace Trades.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            // Add DB
            var connectionString = builder.Configuration.GetConnectionString("TradesDB") ?? throw new InvalidOperationException("Connection string" + "'TradesDB' not found.");
            builder.Services.AddDbContext<TradesDbContext>(options =>options.UseSqlServer(connectionString));

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
