using System.Threading;
using System.Threading.Tasks;
using BiAi.Models;
using BiAi.Models.Config;

namespace BiAi.Services
{
    public interface ITelegramService
    {
        Task ProcessTriggerAsync(TelegramTriggerConfig telegramTrigger, Image image, CancellationToken cancellationToken);
    }
}