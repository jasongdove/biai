using System.Threading;
using System.Threading.Tasks;
using BiAi.Models.Config;

namespace BiAi.Services
{
    public interface ITriggerService
    {
        Task ProcessTriggersAsync(CameraConfig camera, CancellationToken cancellationToken);
    }
}