namespace BiAi.Models
{
    public class CameraConfig
    {
        private string[] _relevantObjects;
        public bool Enabled { get; set; }
        public string Name { get; set; }

        public string[] RelevantObjects
        {
            // the config binder appears to use null rather than an empty array
            get => _relevantObjects ?? new string[0];
            set => _relevantObjects = value;
        }

        public int LowerCertainty { get; set; }
        public int UpperCertainty { get; set; }
        public string Caption { get; set; }
    }
}