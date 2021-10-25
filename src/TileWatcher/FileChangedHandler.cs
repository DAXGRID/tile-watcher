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
        private readonly TileProcessSetting _tileProcessingSetting;

        public FileChangedHandler(
            ILogger<FileChangedHandler> logger,
            IOptions<FileSeverSetting> fileServerSetting,
            IOptions<TileProcessSetting> tileProcessingSetting)
        {
            _logger = logger;
            _fileServerSetting = fileServerSetting.Value;
            _tileProcessingSetting = tileProcessingSetting.Value;
        }

        public async Task Handle(FileChangedEvent fileChangedEvent)
        {
            // We do this because .NET options are annoying and we cannot use start slash in enviroment variable
            var fullPathNoStartSlash = RemoveStartSlash(fileChangedEvent.FullPath);
            if (!_tileProcessingSetting.Process.ContainsKey(fullPathNoStartSlash))
                return;

            try
            {
                var fileExtension = Path.GetExtension(fileChangedEvent.FullPath);
                if (fileExtension != ".geojson")
                    throw new Exception($"The '{fileExtension}' for file {fileChangedEvent.FullPath} is not valid, only .geojson is valid.");

                var token = BasicAuthToken(_fileServerSetting.Username, _fileServerSetting.Password);
                var fileNameGeoJson = Path.GetFileName(fileChangedEvent.FullPath);
                using var webClient = new WebClient();
                webClient.Headers.Add("Authorization", $"Basic {token}");
                await webClient.DownloadFileTaskAsync(new Uri($"{_fileServerSetting.Uri}{fileChangedEvent.FullPath}"), $"/tmp/{fileNameGeoJson}");

                var tippeCannoeArgs = _tileProcessingSetting.Process[fullPathNoStartSlash];
                var stdoutTippecanoe = TileProcess.RunTippecanoe(tippeCannoeArgs);
                _logger.LogInformation(stdoutTippecanoe);

                var fileNameVectorTiles = $"{Path.GetFileNameWithoutExtension(fileNameGeoJson)}.mbtiles";
                File.Move($"/tmp/{fileNameVectorTiles}", $"{_tileProcessingSetting.Destination}/{fileNameVectorTiles}", true);

                var stdoutReload = TileProcess.ReloadMbTileServer();
                _logger.LogInformation(stdoutReload);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message + "\n" + ex.StackTrace);
            }
        }

        private static string RemoveStartSlash(string path)
        {
            if (path.Length == 0)
                return string.Empty;

            return path[0] switch
            {
                '/' => path.Remove(0, 1),
                _ => path
            };
        }

        private static string BasicAuthToken(string username, string password)
        {
            return Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes($"{username}:{password}"));
        }
    }
}
