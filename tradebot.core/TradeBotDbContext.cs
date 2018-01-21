using Microsoft.EntityFrameworkCore;

namespace tradebot.core
{
    public class TradeBotDbContext : DbContext
    {
        public DbSet<TestMessage> TestMessages { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=localhost;Database=eventbus;User Id=sa;Password=NashTech@123;");
        }
    }

    public class TestMessage
    {
        public int Id { get; set; }
        public string Message { get; set; }
    }
}