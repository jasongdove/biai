using System;
using LanguageExt;

namespace BiAi.Models.Config;

public class TelegramTriggerConfig
{
    public string Caption { get; set; }
    public int CooldownSeconds { get; set; }

    public Option<DateTime> NextTrigger { get; set; }

    public bool IsInCooldown(Image image) =>
        NextTrigger
            .Some(next => image.Timestamp < next)
            .None(false);
}
