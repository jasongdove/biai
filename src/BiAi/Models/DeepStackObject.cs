using Newtonsoft.Json;

namespace BiAi.Models;

public class DeepStackObject
{
    [JsonProperty("label")]
    public string Label { get; set; }

    [JsonProperty("confidence")]
    public float Confidence { get; set; }

    [JsonProperty("y_min")]
    public int YMin { get; set; }

    [JsonProperty("x_min")]
    public int XMin { get; set; }

    [JsonProperty("y_max")]
    public int YMax { get; set; }

    [JsonProperty("x_max")]
    public int XMax { get; set; }
}
