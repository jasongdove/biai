using System.Threading.Tasks;
using BiAi.Models;

namespace BiAi.Services
{
    public interface IDeepStackService
    {
        Task<DeepStackResponse> DetectAsync(string fullPath);
    }
}