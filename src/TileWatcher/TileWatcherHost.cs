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

        public TileWatcherHost(
            ILogger<TileWatcherHost> logger,
            IHostApplicationLifetime hostApplicationLifetime,
            IFileNotificationConsumer fileNotficationConsumer)
        {
            _logger = logger;
            _hostApplicationLifeTime = hostApplicationLifetime;
            _fileNotificationConsumer = fileNotficationConsumer;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Starting {nameof(TileWatcherHost)}.");
            _hostApplicationLifeTime.ApplicationStarted.Register(OnStarted);
            _hostApplicationLifeTime.ApplicationStopping.Register(OnStopped);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private void MarkAsReady()
        {
            File.Create("/tmp/healthy");
        }

        private void OnStarted()
        {
            _fileNotificationConsumer.Start();
            MarkAsReady();
        }

        private void OnStopped()
        {
            _fileNotificationConsumer.Dispose();
            _logger.LogInformation("Stopped");
        }
    }
}
