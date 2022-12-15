using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

namespace TileWatcher
{
    internal class TileWatcherHost : IHostedService
    {
        private readonly ILogger<TileWatcherHost> _logger;
        private readonly IHostApplicationLifetime _hostApplicationLifeTime;
        private readonly IFileNotificationConsumer _fileNotificationConsumer;
        private readonly IInitialTileProcss _initialTileProcess;

        public TileWatcherHost(
            ILogger<TileWatcherHost> logger,
            IHostApplicationLifetime hostApplicationLifetime,
            IFileNotificationConsumer fileNotficationConsumer,
            IInitialTileProcss initialTileProcess)
        {
            _logger = logger;
            _hostApplicationLifeTime = hostApplicationLifetime;
            _fileNotificationConsumer = fileNotficationConsumer;
            _initialTileProcess = initialTileProcess;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Starting {nameof(TileWatcherHost)}.");

            _logger.LogInformation("Starting initial tile process.");
            await _initialTileProcess.Start();

            _hostApplicationLifeTime.ApplicationStarted.Register(OnStarted);
            _hostApplicationLifeTime.ApplicationStopping.Register(OnStopped);

            await Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
        }

        private void MarkAsReady()
        {
            File.Create("/tmp/healthy");
        }

        private void OnStarted()
        {
            _logger.LogInformation("Starting file notification consumer.");
            _fileNotificationConsumer.Start();
            MarkAsReady();
        }

        private void OnStopped()
        {
            _logger.LogInformation("Stopped");
        }
    }
}
