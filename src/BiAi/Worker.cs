using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BiAi.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace BiAi
{
    public class Worker : BackgroundService
    {
        private static readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1,1);
        
        private readonly ILogger<Worker> _logger;
        private readonly IDeepStackService _deepStackService;
        private readonly FileSystemWatcher _watcher;

        public Worker(ILogger<Worker> logger, IConfiguration configuration, IDeepStackService deepStackService)
        {
            _logger = logger;
            _deepStackService = deepStackService;

            _watcher = new FileSystemWatcher
            {
                Path = configuration["ImagePath"],
                Filter = configuration["ImageFilter"]
            };
            
            _watcher.Created += OnCreatedAsync;
        }
        
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.CompletedTask;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _watcher.EnableRaisingEvents = true;
            return base.StartAsync(cancellationToken);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _watcher.EnableRaisingEvents = false;
            return base.StopAsync(cancellationToken);
        }

        public override void Dispose()
        {
            _watcher.Dispose();
            base.Dispose();
        }

        private async void OnCreatedAsync(object sender, FileSystemEventArgs e)
        {
            await _semaphoreSlim.WaitAsync();
            try
            {
                _logger.LogInformation($"File {e.FullPath} was created");
                var response = await _deepStackService.DetectAsync(e.FullPath);
                if (response?.Success == true)
                {
                    _logger.LogInformation($"DeepStack success: {JsonConvert.SerializeObject(response)}");
                }
                else
                {
                    _logger.LogWarning("DeepStack failure");
                }
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }
    }
}