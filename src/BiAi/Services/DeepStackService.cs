using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using BiAi.Models;
using LanguageExt;
using static LanguageExt.Prelude;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace BiAi.Services
{
    public class DeepStackService : IDeepStackService
    {
        private readonly ILogger<DeepStackService> _logger;
        private readonly HttpClient _httpClient;

        private readonly Uri _deepStackEndpoint;

        public DeepStackService(ILogger<DeepStackService> logger, IConfiguration configuration, IHttpClientFactory clientFactory)
        {
            _logger = logger;
            _httpClient = clientFactory.CreateClient();
            
            _deepStackEndpoint = new Uri(configuration["DeepStackEndpoint"]);
        }

        public async Task<Option<DeepStackResponse>> DetectAsync(string fullPath)
        {
            // TODO: retry this w/backoff in case image is still being written
            try
            {
                await using var stream = File.OpenRead(fullPath);
                using var streamContent = new StreamContent(stream);

                using var request = new MultipartFormDataContent();
                request.Add(streamContent, "image", Path.GetFileName(fullPath));

                var response = await _httpClient.PostAsync(_deepStackEndpoint, request);
                var jsonString = await response.Content.ReadAsStringAsync();
                return Some(JsonConvert.DeserializeObject<DeepStackResponse>(jsonString));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process image at {imagePath}", fullPath);
            }

            return None;
        }
    }
}