using System;

namespace BiAi.Models.Config
{
    public class TelegramTriggerConfig
    {
        public string Caption { get; set; }
        public int CooldownSeconds { get; set; }
        
        public DateTime LastTrigger { get; set; }
    }
}