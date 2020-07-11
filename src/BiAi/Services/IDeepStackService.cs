using System.Threading.Tasks;
using BiAi.Models;
using LanguageExt;

namespace BiAi.Services
{
    public interface IDeepStackService
    {
        Task<Option<DeepStackResponse>> DetectAsync(string fullPath);
    }
}