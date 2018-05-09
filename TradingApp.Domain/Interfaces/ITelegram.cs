using System.Threading.Tasks;

namespace TradingApp.Domain.Interfaces
{
    public interface ITelegram
    {
        long GetChatId();
        void SendMessage(string message);
    }
}