using System;

namespace BiAi.Models.Config;

public class TelegramConfig
{
    private long[] _chatIds;
    public string Token { get; set; }

    public long[] ChatIds
    {
        // the config binder appears to use null rather than an empty array
        get => _chatIds ?? Array.Empty<long>();
        set => _chatIds = value;
    }
}
