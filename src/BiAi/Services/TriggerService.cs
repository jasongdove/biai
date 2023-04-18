using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using BiAi.Models;
using BiAi.Models.Config;

namespace BiAi.Services;

public class TriggerService : ITriggerService
{
    private readonly HttpClient _httpClient;

    public TriggerService(IHttpClientFactory clientFactory) => _httpClient = clientFactory.CreateClient();

    public async Task ProcessTriggersAsync(CameraConfig camera, Image image, CancellationToken cancellationToken)
    {
        foreach (TriggerConfig trigger in camera.Triggers.Where(t => !t.IsInCooldown(image)))
        {
            await _httpClient.GetAsync(trigger.Url, cancellationToken);
            trigger.NextTrigger = image.Timestamp + TimeSpan.FromSeconds(trigger.CooldownSeconds);
        }
    }
}
