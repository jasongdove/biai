using System.Threading;
using System.Threading.Tasks;
using BiAi.Models;
using LanguageExt;
using LanguageExt.Common;

namespace BiAi.Services
{
    public interface IDeepStackService
    {
        Task<Either<Error, DeepStackResponse>> DetectAsync(string fullPath, CancellationToken cancellationToken);
    }
}