using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Topos.Config;
using TileWatcher.Serialize;
using System.Collections.Generic;

namespace TileWatcher
{
    internal class FileNotificationConsumer : IFileNotificationConsumer
    {
        private IDisposable? _consumer;
        private KafkaSetting _kafkaSetting;
        private ILogger<FileNotificationConsumer> _logger;
        private Dictionary<string, string> _fileSha256;

        public FileNotificationConsumer(
            IOptions<KafkaSetting> kafkaSetting,
            ILogger<FileNotificationConsumer> logger)
        {
            if (kafkaSetting is null || kafkaSetting.Value is null)
                throw new ArgumentNullException($"{nameof(KafkaSetting)} being null is not valid.");
            _kafkaSetting = kafkaSetting.Value;
            _logger = logger;
            _fileSha256 = new Dictionary<string, string>();
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
                                if (checksumChanged(fileChangedEvent.FullPath, fileChangedEvent.Sha256CheckSum, _fileSha256))
                                {
                                    FileChangedHandler.Handle(fileChangedEvent);
                                }
                                else
                                {
                                    _logger.LogInformation($"Filechanged with fullpath: '{fileChangedEvent.FullPath}' has same checksum so no updates");
                                }
                                break;
                        }
                    }
                }).Start();
        }

        private static bool checksumChanged(string fullPath, string sha256CheckSum, Dictionary<string, string> lastFilesSha256)
        {
            return !(!lastFilesSha256.ContainsKey(fullPath)
                   || sha256CheckSum != lastFilesSha256[fullPath]);
        }

        public void Dispose()
        {
            if (_consumer is not null)
                _consumer.Dispose();
        }
    }
}
