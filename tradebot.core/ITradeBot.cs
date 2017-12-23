using System.Threading.Tasks;

namespace tradebot.core
{
    public interface ITradeBot
    {
         Task Execute();
    }
}