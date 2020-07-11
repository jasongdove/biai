namespace BiAi.Models
{
    public class DeepStackResponse
    {
        public bool success { get; set; }
        public DeepStackObject[] predictions { get; set; }
    }
}