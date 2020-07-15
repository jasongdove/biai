using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using BiAi.Models.Config;

namespace BiAi.Services
{
    public class TriggerService : ITriggerService
    {
        private readonly HttpClient _httpClient;

        public TriggerService(IHttpClientFactory clientFactory)
        {
            _httpClient = clientFactory.CreateClient();
        }

        public async Task ProcessTriggersAsync(CameraConfig camera, CancellationToken cancellationToken)
        {
            foreach (var trigger in camera.Triggers.Where(t => !IsTriggerInCooldown(t)))
            {
                await _httpClient.GetAsync(trigger.Url, cancellationToken);
                trigger.LastTrigger = DateTime.Now;
            }
        }

        private static bool IsTriggerInCooldown(TriggerConfig trigger)
        {
            var nextTrigger = trigger.LastTrigger + TimeSpan.FromSeconds(trigger.CooldownSeconds);
            return DateTime.Now < nextTrigger;
        }
    }
}