using TileWatcher.Config;
using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace TileWatcher
{
    internal class FileChangedHandler : IFileChangedHandler
    {
        private readonly ILogger<FileChangedHandler> _logger;
        private readonly FileSeverSetting _fileServerSetting;
        private readonly TileProcessingSetting _tileProcessingSetting;

        public FileChangedHandler(
            ILogger<FileChangedHandler> logger,
            IOptions<FileSeverSetting> fileServerSetting,
            IOptions<TileProcessingSetting> tileProcessingSetting)
        {
            _logger = logger;
            _fileServerSetting = fileServerSetting.Value;
            _tileProcessingSetting = tileProcessingSetting.Value;
        }

        public async Task Handle(FileChangedEvent fileChangedEvent)
        {
            // TODO clean this up make sure that the settings cannot be null or empty from the start.
            if (string.IsNullOrWhiteSpace(_fileServerSetting.Username))
                throw new Exception("Fileserver settings username cannot be null, empty or whitespace");
            if (string.IsNullOrWhiteSpace(_fileServerSetting.Password))
                throw new Exception("Fileserver settings password cannot be null, empty or whitespace");
            if (string.IsNullOrWhiteSpace(_fileServerSetting.Uri))
                throw new Exception("Fileserver settings uri cannot be null, empty or whitespace");

            var fileExtension = Path.GetExtension(fileChangedEvent.FullPath);
            if (fileExtension != ".geojson")
                throw new Exception($"The '{fileExtension}' for file {fileChangedEvent.FullPath} is not valid, only .geojson is valid.");

            var token = BasicAuthToken(_fileServerSetting.Username, _fileServerSetting.Password);
            var fileNameGeoJson = Path.GetFileName(fileChangedEvent.FullPath);
            using var webClient = new WebClient();
            webClient.Headers.Add("Authorization", $"Basic {token}");
            await webClient.DownloadFileTaskAsync(new Uri($"{_fileServerSetting.Uri}{fileChangedEvent.FullPath}"), $"/tmp/{fileNameGeoJson}");

            var fileNameVectorTiles = $"{Path.GetFileNameWithoutExtension(fileNameGeoJson)}.mbtiles";

            TileProcess.RunTippecanoe(_tileProcessingSetting.TippeCanoeArgs);
            File.Move($"/tmp/{fileNameVectorTiles}", $"{_tileProcessingSetting.Destination}/{fileNameVectorTiles}", true);
            TileProcess.SendReloadSignal(1);
        }

        private static string BasicAuthToken(string username, string password)
        {
            return Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes($"{username}:{password}"));
        }
    }
}
