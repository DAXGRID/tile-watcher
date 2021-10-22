using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Topos.Config;
using TileWatcher.Serialize;

namespace TileWatcher
{
    internal class FileNotificationConsumer : IFileNotificationConsumer
    {
        private IDisposable? _consumer;
        private KafkaSetting _kafkaSetting;
        private ILogger<FileNotificationConsumer> _logger;

        public FileNotificationConsumer(
            IOptions<KafkaSetting> kafkaSetting,
            ILogger<FileNotificationConsumer> logger)
        {
            if (kafkaSetting is null || kafkaSetting.Value is null)
                throw new ArgumentNullException($"{nameof(KafkaSetting)} being null is not valid.");
            _kafkaSetting = kafkaSetting.Value;
            _logger = logger;
        }

        public void Start()
        {
            _consumer = Configure.Consumer(_kafkaSetting.Consumer, c => c.UseKafka(_kafkaSetting.Server))
                .Serialization(s => s.FileNotificationSerializer())
                .Topics(t => t.Subscribe(_kafkaSetting.Topic))
                .Positions(p =>
                {
                    p.SetInitialPosition(StartFromPosition.Now);
                    p.StoreInMemory();
                })
                .Handle(async (messages, context, token) =>
                {
                    _logger.LogInformation("Received message");
                    foreach (var message in messages)
                    {
                        switch (message.Body)
                        {
                            case FileChangedEvent fileChangedEvent:
                                _logger.LogInformation($"Received file changed event {fileChangedEvent.Sha256CheckSum}");
                                break;
                        }
                    }
                }).Start();
        }

        public void Dispose()
        {
            if (_consumer is not null)
                _consumer.Dispose();
        }
    }
}
