using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using BiAi.Models;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using static LanguageExt.Prelude;

namespace BiAi.Services;

public class DeepStackService : IDeepStackService
{
    private readonly Uri _deepStackEndpoint;
    private readonly HttpClient _httpClient;

    public DeepStackService(IConfiguration configuration, IHttpClientFactory clientFactory)
    {
        _httpClient = clientFactory.CreateClient();
        _deepStackEndpoint = new Uri(configuration["DeepStackEndpoint"]);
    }

    public async Task<Either<Error, DeepStackResponse>> DetectAsync(
        string fullPath,
        CancellationToken cancellationToken)
    {
        // TODO: retry this w/backoff in case image is still being written
        try
        {
            await using FileStream stream = File.OpenRead(fullPath);
            using var streamContent = new StreamContent(stream);

            using var request = new MultipartFormDataContent();
            request.Add(streamContent, "image", Path.GetFileName(fullPath));

            HttpResponseMessage response = await _httpClient.PostAsync(_deepStackEndpoint, request, cancellationToken);
            string jsonString = await response.Content.ReadAsStringAsync(cancellationToken);
            return Right(JsonConvert.DeserializeObject<DeepStackResponse>(jsonString));
        }
        catch (Exception ex)
        {
            return Error.New($"Failed to process image at {fullPath}", ex);
        }
    }
}
