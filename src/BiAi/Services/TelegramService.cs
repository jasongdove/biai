using System.IO;
using System.Threading.Tasks;
using BiAi.Models;
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
        
        public async Task SendAlarmAsync(string fullPath)
        {
            // TODO: make this more functional
            var chatIds = _telegramConfig.ChatIds.ToSeq();
            if (!chatIds.IsEmpty)
            {
                var client = new TelegramBotClient(_telegramConfig.Token);
                await using var stream = File.OpenRead(fullPath);
                var message = await client.SendPhotoAsync(chatIds.Head, stream);
                var photos = message.Photo.ToSeq();
                if (!photos.IsEmpty)
                {
                    var fileId = photos.Head.FileId;
                    foreach (var chatId in chatIds.Tail)
                    {
                        await client.SendPhotoAsync(chatId, fileId);
                    }
                }
            }
        }
    }
}