using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenFTTH.NotificationClient;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading.Tasks;
using TileWatcher.Config;

using NotificationClient = OpenFTTH.NotificationClient.Client;

namespace TileWatcher
{
    internal class FileNotificationConsumer : IFileNotificationConsumer
    {
        private readonly ILogger<FileNotificationConsumer> _logger;
        private readonly Dictionary<string, string> _fileSha256;
        private readonly IFileChangedHandler _fileChangedHandler;
        private readonly NotificationClient _notificationClient;

        public FileNotificationConsumer(
            IOptions<NotificationServerSetting> notificationSetting,
            ILogger<FileNotificationConsumer> logger,
            IFileChangedHandler fileChangedHandler)
        {
            _logger = logger;
            _fileChangedHandler = fileChangedHandler;
            _fileSha256 = new Dictionary<string, string>();

            var ipAddress = Dns.GetHostEntry(notificationSetting.Value.Domain).AddressList
                .First(x => x.AddressFamily == AddressFamily.InterNetwork);

            _notificationClient = new NotificationClient(
                ipAddress,
                notificationSetting.Value.Port);
        }

        public async Task Start()
        {
            var notificationCh = _notificationClient.Connect();

            await foreach (var notification in notificationCh.ReadAllAsync())
            {
                if (string.CompareOrdinal(notification.Type, "FileChangedEvent") == 0)
                {
                    var fileChangedEvent = JsonSerializer
                        .Deserialize<FileChangedEvent>(notification.Body);

                    _logger.LogInformation(
                        "Received message with {FullPath} and {CheckSum}.",
                        fileChangedEvent.FullPath,
                        fileChangedEvent.Sha256CheckSum);

                    if (!CheckSumEqual(fileChangedEvent.FullPath, fileChangedEvent.Sha256CheckSum, _fileSha256))
                    {
                        await _fileChangedHandler.Handle(fileChangedEvent);
                        if (_fileSha256.ContainsKey(fileChangedEvent.FullPath))
                        {
                            _fileSha256[fileChangedEvent.FullPath] = fileChangedEvent.Sha256CheckSum;
                        }
                        else
                        {
                            _fileSha256.Add(fileChangedEvent.FullPath, fileChangedEvent.Sha256CheckSum);
                        }

                        // Send out that the tileset has been refreshed.
                        _notificationClient.Send(
                            new OpenFTTH.NotificationClient.Notification(
                                "TilesetUpdated",
                                JsonSerializer.Serialize(new TilesetUpdated(Path.GetFileName(fileChangedEvent.FullPath)))
                            )
                        );
                    }
                    else
                    {
                        _logger.LogInformation(
                            "Message with {FullPath} has the same {CheckSum}, skipping file change.",
                            fileChangedEvent.FullPath,
                            fileChangedEvent.Sha256CheckSum);
                    }
                }
            }
        }

        private static bool CheckSumEqual(
            string fullPath,
            string sha256CheckSum,
            Dictionary<string, string> lastFilesSha256)
        {
            return lastFilesSha256.ContainsKey(fullPath) && sha256CheckSum == lastFilesSha256[fullPath];
        }
    }
}

public record TilesetUpdated
{
    public string TilesetName { get; init; }

    public TilesetUpdated(string tilesetName)
    {
        TilesetName = tilesetName;
    }
}
