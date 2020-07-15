using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BiAi.Models;
using BiAi.Models.Config;
using LanguageExt;
using Microsoft.Extensions.Logging;

namespace BiAi.Services
{
    public class ImageProcessor : IImageProcessor
    {
        private readonly ILogger<ImageProcessor> _logger;
        private readonly IDeepStackService _deepStackService;
        private readonly ITelegramService _telegramService;
        private readonly ITriggerService _triggerService;

        public ImageProcessor(ILogger<ImageProcessor> logger, IDeepStackService deepStackService,
            ITelegramService telegramService, ITriggerService triggerService)
        {
            _logger = logger;
            _deepStackService = deepStackService;
            _telegramService = telegramService;
            _triggerService = triggerService;
        }

        public async Task ProcessImageAsync(CameraConfig camera, string fullPath, CancellationToken cancellationToken)
        {
            _logger.LogDebug(
                "Image at {imagePath} was created and matched to camera {camera}",
                fullPath,
                camera.Name);
            
            // TODO: pull timestamp from image for more accurate cooldowns

            var response = await _deepStackService.DetectAsync(fullPath, cancellationToken);
            await response
                .Right(async r => await ProcessDeepStackResponseAsync(camera, r, fullPath, cancellationToken))
                .Left(async error => await Task.Run(() => _logger.LogWarning(error.Message), cancellationToken));
        }

        private async Task ProcessDeepStackResponseAsync(CameraConfig camera, DeepStackResponse response,
            string fullPath, CancellationToken cancellationToken)
        {
            if (response.Success)
            {
                var relevantObjects = GetRelevantObjects(camera, response);
                if (!relevantObjects.IsEmpty)
                {
                    _logger.LogDebug(
                        "Detected relevant objects {objects}",
                        relevantObjects.Map(o => new { o.Label, o.Confidence }));

                    if (camera.TelegramTrigger != null)
                    {
                        await _telegramService.ProcessTriggerAsync(camera.TelegramTrigger, fullPath, cancellationToken);
                    }

                    await _triggerService.ProcessTriggersAsync(camera, cancellationToken);
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