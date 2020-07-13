using System.Threading;
using System.Threading.Tasks;
using BiAi.Models;

namespace BiAi.Services
{
    public interface ITelegramService
    {
        Task SendAlarmAsync(CameraConfig camera, string fullPath, CancellationToken cancellationToken);
    }
}