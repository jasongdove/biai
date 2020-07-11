using Newtonsoft.Json;

namespace BiAi.Models
{
    public class DeepStackResponse
    {
        [JsonProperty("success")]
        public bool Success { get; set; }
        
        [JsonProperty("predictions")]
        public DeepStackObject[] Predictions { get; set; }
    }
}