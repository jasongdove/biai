using System.Threading.Tasks;
using BiAi.Models;

namespace BiAi.Services
{
    public interface IImageProcessor
    {
        Task ProcessImageAsync(CameraConfig camera, string fullPath);
    }
}