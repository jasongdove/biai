using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using BiAi.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace BiAi.Services
{
    public class DeepStackService : IDeepStackService
    {
        private readonly ILogger<DeepStackService> _logger;
        private readonly IHttpClientFactory _clientFactory;

        private readonly Uri _deepStackEndpoint;

        public DeepStackService(ILogger<DeepStackService> logger, IConfiguration configuration, IHttpClientFactory clientFactory)
        {
            _logger = logger;
            _clientFactory = clientFactory;
            
            _deepStackEndpoint = new Uri(configuration["DeepStackEndpoint"]);
        }

        public async Task<DeepStackResponse> DetectAsync(string fullPath)
        {
            // TODO: retry this w/backoff in case image is still being written
            try
            {
                await using var stream = File.OpenRead(fullPath);
                var client = _clientFactory.CreateClient();

                var request = new MultipartFormDataContent();
                request.Add(new StreamContent(stream), "image", Path.GetFileName(fullPath));

                var response = await client.PostAsync(_deepStackEndpoint, request);
                var jsonString = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<DeepStackResponse>(jsonString);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to open image {fullPath}");
            }

            return null;
        }
    }
}