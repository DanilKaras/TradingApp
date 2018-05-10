using System.Threading.Tasks;

namespace TradingApp.Domain.Interfaces
{
    public interface ITelegram
    {
        long GetChatId();
        Task SendMessage(string message);
    }
}