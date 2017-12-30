using System.Threading.Tasks;

namespace tradebot.core.helper
{
    public interface IEmailHelper
    {
        Task SendEmail(string subject, string content);
    }
}