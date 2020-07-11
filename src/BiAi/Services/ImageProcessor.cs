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

        public ImageProcessor(ILogger<ImageProcessor> logger, IDeepStackService deepStackService)
        {
            _logger = logger;
            _deepStackService = deepStackService;
        }
        
        public async Task ProcessImageAsync(CameraConfig camera, string fullPath)
        {
            _logger.LogDebug(
                "File [{file}] was created and matched to camera [{camera}]",
                fullPath,
                camera.Name);
                        
            var response = await _deepStackService.DetectAsync(fullPath);
            await response
                .Some(async r => await ProcessDeepStackResponseAsync(camera, r))
                .None(async () => _logger.LogWarning("No response from DeepStackService?"));
        }
        
        private async Task ProcessDeepStackResponseAsync(CameraConfig camera, DeepStackResponse response)
        {
            if (response.Success)
            {
                _logger.LogInformation("DeepStack success.");
                var relevantObjects = GetRelevantObjects(camera, response);
                if (!relevantObjects.IsEmpty)
                {
                    _logger.LogInformation("ALERT!");
                }
            }
            else
            {
                _logger.LogWarning("DeepStack did not detect any objects");
            }
        }
        
        private static Seq<DeepStackObject> GetRelevantObjects(CameraConfig camera, DeepStackResponse response)
        {
            return new Seq<DeepStackObject>(response.Predictions)
                .Filter(p => camera.RelevantObjects.Contains(p.Label)
                             && p.Confidence * 100 >= camera.LowerCertainty
                             && p.Confidence * 100 <= camera.UpperCertainty);
        }
    }
}