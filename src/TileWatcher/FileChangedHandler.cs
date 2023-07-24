using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Threading.Tasks;
using TileWatcher.Config;

namespace TileWatcher
{
    internal class FileChangedHandler : IFileChangedHandler
    {
        private readonly ILogger<FileChangedHandler> _logger;
        private readonly FileServerSetting _fileServerSetting;
        private readonly TileProcessSetting _tileProcessingSetting;
        private readonly IInitialTileProcss _initialTileProcess;

        public FileChangedHandler(
            ILogger<FileChangedHandler> logger,
            IOptions<FileServerSetting> fileServerSetting,
            IOptions<TileProcessSetting> tileProcessingSetting,
            IInitialTileProcss initialTileProcess)
        {
            _logger = logger;
            _fileServerSetting = fileServerSetting.Value;
            _tileProcessingSetting = tileProcessingSetting.Value;
            _initialTileProcess = initialTileProcess;
        }

        public async Task Handle(FileChangedEvent fileChangedEvent)
        {
            // We do this because .NET options are annoying and we cannot use start slash in enviroment variable
            var fullPathNoStartSlash = RemoveStartSlash(fileChangedEvent.FullPath);
            if (!_tileProcessingSetting.Process.ContainsKey(fullPathNoStartSlash))
                return;

            try
            {
                if (!TileProcess.IsGeoJsonFile(fileChangedEvent.FullPath) && !TileProcess.IsMbTileFile(fileChangedEvent.FullPath))
                {
                    throw new Exception($"The {fileChangedEvent.FullPath} is invalid, only supports .geojson and .mbtiles.");
                }

                var (username, password, uri) = _fileServerSetting;
                await TileProcess.DownloadFile(username, password, uri, fileChangedEvent.FullPath);

                if (TileProcess.IsGeoJsonFile(fileChangedEvent.FullPath))
                {
                    var tippeCannoeArgs = _tileProcessingSetting.Process[fullPathNoStartSlash];
                    TileProcess.RunTippecanoe(tippeCannoeArgs);
                }
                else if (TileProcess.IsMbTileFile(fileChangedEvent.FullPath))
                {
                    // Do nothing since the .mbtiles file has already been processed.
                }
                else
                {
                    throw new Exception(
                        $"Could not handle file extension of type: {Path.GetExtension(fileChangedEvent.FullPath)}");
                }

                var fileNameVectorTiles = TileProcess.ChangeFileExtensionName(fileChangedEvent.FullPath, ".mbtiles");
                File.Move($"/tmp/{fileNameVectorTiles}", $"{_tileProcessingSetting.Destination}/{fileNameVectorTiles}", true);

                TileProcess.ReloadMbTileServer();
                _logger.LogInformation($"Finished processing '{fileChangedEvent.FullPath}'.");
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
    }
}
