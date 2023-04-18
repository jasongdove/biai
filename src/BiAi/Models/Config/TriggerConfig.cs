using System;
using LanguageExt;

namespace BiAi.Models.Config;

public class TriggerConfig
{
    public string Url { get; set; }
    public int CooldownSeconds { get; set; }

    public Option<DateTime> NextTrigger { get; set; }

    public bool IsInCooldown(Image image) =>
        NextTrigger
            .Some(next => image.Timestamp < next)
            .None(false);
}
