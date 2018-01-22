using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace tradebot.api
{
    public class TradeBotDbContext : DbContext
    {
        private IConfiguration _configuration;
        public TradeBotDbContext(IConfiguration configuration)
        {
            this._configuration = configuration;
        }

        public DbSet<TestMessage> TestMessages { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // optionsBuilder.UseSqlServer(_configuration["CAP:DatabaseConnectionString"]);
            optionsBuilder.UseNpgsql(_configuration["CAP:DatabaseConnectionString"]);
        }
    }

    public class TestMessage
    {
        public int Id { get; set; }
        public string Message { get; set; }
    }
}