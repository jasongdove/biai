using System;

namespace BiAi.Models.Config
{
    public class TriggerConfig
    {
        public string Url { get; set; }
        public int CooldownSeconds { get; set; }
        
        public DateTime LastTrigger { get; set; }
    }
}