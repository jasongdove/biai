using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BiAi.Models;
using BiAi.Models.Config;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using static LanguageExt.Prelude;
using File = System.IO.File;

namespace BiAi.Services;

public class TelegramService : ITelegramService
{
    private readonly ILogger<TelegramService> _logger;
    private readonly TelegramConfig _telegramConfig;

    public TelegramService(ILogger<TelegramService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _telegramConfig = configuration.GetSection("Telegram").Get<TelegramConfig>();
    }

    public async Task ProcessTriggerAsync(
        TelegramTriggerConfig telegramTrigger,
        Image image,
        CancellationToken cancellationToken)
    {
        if (!telegramTrigger.IsInCooldown(image))
        {
            Option<Error> result = await SendPhotoAsync(telegramTrigger, image.FullPath, cancellationToken);
            await result.IfSomeAsync(async _ => await SendTextAsync(telegramTrigger, cancellationToken));
            telegramTrigger.NextTrigger = image.Timestamp + TimeSpan.FromSeconds(telegramTrigger.CooldownSeconds);
        }
    }

    private async Task<Option<Error>> SendPhotoAsync(
        TelegramTriggerConfig telegramTrigger,
        string fullPath,
        CancellationToken cancellationToken)
    {
        // TODO: make this more functional
        var chatIds = _telegramConfig.ChatIds.ToSeq();
        if (chatIds.IsEmpty)
        {
            return None;
        }

        var client = new TelegramBotClient(_telegramConfig.Token);
        await using FileStream stream = File.OpenRead(fullPath);
        Option<string> fileId = None;
        var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(TimeSpan.FromSeconds(5));
        try
        {
            Message message = await client.SendPhotoAsync(
                chatIds.Head,
                stream,
                telegramTrigger.Caption,
                cancellationToken: cts.Token);
            var photos = message.Photo.ToSeq();
            if (!photos.IsEmpty)
            {
                fileId = photos.Head.FileId;
            }
        }
        catch (Exception ex)
        {
            // we only consider the send to have failed
            // if the initial upload fails since subsequent
            // sends use a file id

            // don't bother returning an error if the child
            // task wasn't canceled (meaning we're shutting down)
            return cts.IsCancellationRequested
                ? Some(Error.New(ex))
                : None;
        }

        await fileId.IfSomeAsync(
            async f =>
            {
                foreach (long chatId in chatIds.Tail)
                {
                    await client.SendPhotoAsync(
                        chatId,
                        f,
                        telegramTrigger.Caption,
                        cancellationToken: cancellationToken);
                }
            });

        return None;
    }

    private async Task SendTextAsync(TelegramTriggerConfig telegramTrigger, CancellationToken cancellationToken)
    {
        var chatIds = _telegramConfig.ChatIds.ToSeq();
        if (!chatIds.IsEmpty)
        {
            var client = new TelegramBotClient(_telegramConfig.Token);
            var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(5));
            try
            {
                await client.SendTextMessageAsync(
                    chatIds.Head,
                    telegramTrigger.Caption,
                    cancellationToken: cts.Token);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send fallback text to telegram");
            }
        }
    }
}
