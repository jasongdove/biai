using System;

namespace BiAi.Models.Config;

public class CameraConfig
{
    private string[] _relevantObjects;
    private TriggerConfig[] _triggers;

    public bool Enabled { get; set; }
    public string Name { get; set; }

    public string[] RelevantObjects
    {
        // the config binder appears to use null rather than an empty array
        get => _relevantObjects ?? Array.Empty<string>();
        set => _relevantObjects = value;
    }

    public int LowerCertainty { get; set; }
    public int UpperCertainty { get; set; }
    public TelegramTriggerConfig TelegramTrigger { get; set; }

    public TriggerConfig[] Triggers
    {
        // the config binder appears to use null rather than an empty array
        get => _triggers ?? Array.Empty<TriggerConfig>();
        set => _triggers = value;
    }
}
