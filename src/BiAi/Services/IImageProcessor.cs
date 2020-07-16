using System.Threading;
using System.Threading.Tasks;
using BiAi.Models;
using BiAi.Models.Config;

namespace BiAi.Services
{
    public interface IImageProcessor
    {
        Task ProcessImageAsync(CameraConfig camera, Image image, CancellationToken cancellationToken);
    }
}