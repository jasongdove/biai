using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BiAi.Models.Config;
using BiAi.Services;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BiAi
{
    public class Worker : BackgroundService
    {
        private static readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1,1);
        
        private readonly ILogger<Worker> _logger;
        private readonly IImageProcessor _imageProcessor;
        private readonly FileSystemWatcher _watcher;
        private readonly List<CameraConfig> _cameras;

        private CancellationToken _cancellationToken;

        public Worker(ILogger<Worker> logger, IConfiguration configuration, IImageProcessor imageProcessor)
        {
            _logger = logger;
            _imageProcessor = imageProcessor;

            _watcher = new FileSystemWatcher
            {
                Path = configuration["ImagePath"],
                Filter = configuration["ImageFilter"]
            };
            
            _watcher.Created += OnCreatedAsync;

            _cameras = configuration.GetSection("Cameras").Get<List<CameraConfig>>();
            _logger.LogInformation("Loaded {count} cameras", _cameras.Count);
            foreach (var camera in _cameras)
            {
                var objects = string.Join(", ", camera.RelevantObjects);
                _logger.LogDebug("Camera {camera} relevant objects: {objects}", camera.Name, objects);
            }
        }
        
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _cancellationToken = stoppingToken;
            await Task.CompletedTask;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _watcher.EnableRaisingEvents = true;
            _logger.LogInformation("Watching {folder} for images matching {filter}", _watcher.Path, _watcher.Filter);
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
            await _semaphoreSlim.WaitAsync(_cancellationToken);
            
            // this helps with files that are still being written
            await Task.Delay(TimeSpan.FromMilliseconds(100), _cancellationToken);
            
            try
            {
                await GetCameraForFile(e.FullPath)
                    .Right(async c => await _imageProcessor.ProcessImageAsync(c, e.FullPath, _cancellationToken))
                    .Left(async m => await Task.Run(() => _logger.LogWarning(m.Message), _cancellationToken));
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }

        private Either<Error, CameraConfig> GetCameraForFile(string fullPath)
        {
            return Path.GetFileNameWithoutExtension(fullPath)
                .Split('.')
                .HeadOrNone()
                .Bind(p => _cameras.Filter(c => c.Enabled && c.Name == p).HeadOrNone())
                .ToEither(Error.New($"Could not match camera for image at {fullPath}"));
        }
    }
}