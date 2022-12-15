using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using TileWatcher.Config;

using NotificationClient = OpenFTTH.NotificationClient.Client;

namespace TileWatcher
{
    internal class FileNotificationConsumer : IFileNotificationConsumer
    {
        private readonly NotificationServerSetting _kafkaSetting;
        private readonly ILogger<FileNotificationConsumer> _logger;
        private readonly Dictionary<string, string> _fileSha256;
        private readonly IFileChangedHandler _fileChangedHandler;
        private readonly NotificationClient _notificationClient;

        public FileNotificationConsumer(
            IOptions<NotificationServerSetting> kafkaSetting,
            ILogger<FileNotificationConsumer> logger,
            IFileChangedHandler fileChangedHandler)
        {
            _kafkaSetting = kafkaSetting.Value;
            _logger = logger;
            _fileChangedHandler = fileChangedHandler;
            _fileSha256 = new Dictionary<string, string>();
            // TODO use IP address and domain

            var ipAddress = Dns.GetHostEntry(setting.NotificationServerDomain).AddressList
                .First(x => x.AddressFamily == AddressFamily.InterNetwork);
            _notificationClient = new NotificationClient(IPAddress.Any, 80);
        }

        public async Task Start()
        {
            var notificationCh = _notificationClient.Connect();

            await foreach (var notification in notificationCh.ReadAllAsync())
            {
                if (notification.Type == "MyMessageHeader")
                {
                    // TODO deserialize message
                    //var x = JsonSerializer.Deserialize<MyType>(notification.Body);
                }
            }

            // _consumer = Configure.Consumer(_kafkaSetting.Consumer, c => c.UseKafka(_kafkaSetting.Server))
            //     .Serialization(s => s.FileNotificationSerializer())
            //     .Topics(t => t.Subscribe(_kafkaSetting.Topic))
            //     .Logging(l => l.UseSerilog())
            //     .Positions(p =>
            //     {
            //         p.SetInitialPosition(StartFromPosition.Now);
            //         p.StoreInMemory();
            //     })
            //     .Handle(async (messages, context, token) =>
            //     {
            //         foreach (var message in messages)
            //         {
            //             switch (message.Body)
            //             {
            //                 case FileChangedEvent fileChangedEvent:
            //                     _logger.LogInformation($"Received message with fullpath '{fileChangedEvent.FullPath}' and checksum of '{fileChangedEvent.Sha256CheckSum}'");
            //                     if (!ChecksumEqual(fileChangedEvent.FullPath, fileChangedEvent.Sha256CheckSum, _fileSha256))
            //                     {
            //                         await _fileChangedHandler.Handle(fileChangedEvent);
            //                         if (_fileSha256.ContainsKey(fileChangedEvent.FullPath))
            //                             _fileSha256[fileChangedEvent.FullPath] = fileChangedEvent.Sha256CheckSum;
            //                         else
            //                             _fileSha256.Add(fileChangedEvent.FullPath, fileChangedEvent.Sha256CheckSum);
            //                     }
            //                     else
            //                     {
            //                         _logger.LogInformation($"Message with fullpath: '{fileChangedEvent.FullPath}' has the same checksum so no updates");
            //                     }
            //                     break;
            //             }
            //         }
            //     }).Start();
        }

        private static bool ChecksumEqual(
            string fullPath,
            string sha256CheckSum,
            Dictionary<string, string> lastFilesSha256)
        {
            return lastFilesSha256.ContainsKey(fullPath) && sha256CheckSum == lastFilesSha256[fullPath];
        }
    }
}
