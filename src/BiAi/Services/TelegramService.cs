using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BiAi.Models;
using LanguageExt;
using LanguageExt.Common;
using static LanguageExt.Prelude;
using Microsoft.Extensions.Configuration;
using Telegram.Bot;

namespace BiAi.Services
{
    public class TelegramService : ITelegramService
    {
        private readonly TelegramConfig _telegramConfig;

        public TelegramService(IConfiguration configuration)
        {
            _telegramConfig = configuration.GetSection("Telegram").Get<TelegramConfig>();
        }

        public async Task SendAlarmAsync(CameraConfig camera, string fullPath, CancellationToken cancellationToken)
        {
            var result = await SendPhotoAsync(camera, fullPath, cancellationToken);
            await result.IfSomeAsync(async _ => await SendTextAsync(camera, cancellationToken));
        }

        private async Task<Option<Error>> SendPhotoAsync(CameraConfig camera, string fullPath, CancellationToken cancellationToken)
        {
            // TODO: make this more functional
            var chatIds = _telegramConfig.ChatIds.ToSeq();
            if (chatIds.IsEmpty) return None;
            
            var client = new TelegramBotClient(_telegramConfig.Token);
            await using var stream = File.OpenRead(fullPath);
            Option<string> fileId = None;
            var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(5));
            try
            {
                var message = await client.SendPhotoAsync(chatIds.Head, stream, camera.Caption, cancellationToken: cts.Token);
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

            await fileId.IfSomeAsync(async f =>
            {
                foreach (var chatId in chatIds.Tail)
                {
                    await client.SendPhotoAsync(chatId, f, camera.Caption, cancellationToken: cancellationToken);
                }
            });

            return None;
        }

        private async Task SendTextAsync(CameraConfig camera, CancellationToken cancellationToken)
        {
            var chatIds = _telegramConfig.ChatIds.ToSeq();
            if (!chatIds.IsEmpty)
            {
                var client = new TelegramBotClient(_telegramConfig.Token);
                await client.SendTextMessageAsync(chatIds.Head, camera.Caption, cancellationToken: cancellationToken);
            }
        }
    }
}