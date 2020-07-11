using System.Threading.Tasks;

namespace BiAi.Services
{
    public interface ITelegramService
    {
        Task SendAlarmAsync(string fullPath);
    }
}