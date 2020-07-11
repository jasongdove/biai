namespace BiAi.Models
{
    public class CameraConfig
    {
        public bool Enabled { get; set; }
        public string Name { get; set; }
        public string[] RelevantObjects { get; set; }
        public int LowerCertainty { get; set; }
        public int UpperCertainty { get; set; }
    }
}