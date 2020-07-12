using System.Linq;
using System.Threading.Tasks;
using BiAi.Models;
using LanguageExt;
using Microsoft.Extensions.Logging;

namespace BiAi.Services
{
    public class ImageProcessor: IImageProcessor
    {
        private readonly ILogger<ImageProcessor> _logger;
        private readonly IDeepStackService _deepStackService;
        private readonly ITelegramService _telegramService;

        public ImageProcessor(ILogger<ImageProcessor> logger, IDeepStackService deepStackService, ITelegramService telegramService)
        {
            _logger = logger;
            _deepStackService = deepStackService;
            _telegramService = telegramService;
        }
        
        public async Task ProcessImageAsync(CameraConfig camera, string fullPath)
        {
            _logger.LogDebug(
                "Image at {imagePath} was created and matched to camera {camera}",
                fullPath,
                camera.Name);
                        
            var response = await _deepStackService.DetectAsync(fullPath);
            await response
                .Right(async r => await ProcessDeepStackResponseAsync(camera, r, fullPath))
                .Left(async error => await Task.Run(() => _logger.LogWarning(error.Message)));
        }
        
        private async Task ProcessDeepStackResponseAsync(CameraConfig camera, DeepStackResponse response, string fullPath)
        {
            if (response.Success)
            {
                _logger.LogInformation("DeepStack success.");
                var relevantObjects = GetRelevantObjects(camera, response);
                if (!relevantObjects.IsEmpty)
                {
                    _logger.LogInformation("ALERT!");
                    await _telegramService.SendAlarmAsync(camera, fullPath);
                }
            }
            else
            {
                _logger.LogWarning("DeepStack did not detect any objects");
            }
        }
        
        private static Seq<DeepStackObject> GetRelevantObjects(CameraConfig camera, DeepStackResponse response)
        {
            return response.Predictions.ToSeq()
                .Filter(p => camera.RelevantObjects.Contains(p.Label)
                             && p.Confidence * 100 >= camera.LowerCertainty
                             && p.Confidence * 100 <= camera.UpperCertainty);
        }
    }
}