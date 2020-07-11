using System.IO;
using System.Threading.Tasks;
using BiAi.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types.InputFiles;

namespace BiAi.Services
{
    public class TelegramService : ITelegramService
    {
        private readonly ILogger<TelegramService> _logger;
        private readonly TelegramConfig _telegramConfig;

        public TelegramService(ILogger<TelegramService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _telegramConfig = configuration.GetSection("Telegram").Get<TelegramConfig>();
        }
        
        public async Task SendAlarmAsync(string fullPath)
        {
            await using var stream = File.OpenRead(fullPath);
            var client = new TelegramBotClient(_telegramConfig.Token);
            
            // TODO: record file id and use for multiple chat ids
            foreach (var chatId in _telegramConfig.ChatIds)
            {
                await client.SendPhotoAsync(chatId, new InputOnlineFile(stream, "image.jpg"));
            }
        }
    }
}