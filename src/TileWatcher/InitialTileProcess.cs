using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Threading.Tasks;
using TileWatcher.Config;

namespace TileWatcher
{
    internal class InitialTileProcess : IInitialTileProcss
    {
        private readonly ILogger<InitialTileProcess> _logger;
        private readonly FileServerSetting _fileServerSetting;
        private readonly TileProcessSetting _tileProcessingSetting;

        public InitialTileProcess(
            ILogger<InitialTileProcess> logger,
            IOptions<FileServerSetting> fileServerSetting,
            IOptions<TileProcessSetting> tileProcessingSetting)
        {
            _logger = logger;
            _fileServerSetting = fileServerSetting.Value;
            _tileProcessingSetting = tileProcessingSetting.Value;
        }

        public async Task Start()
        {
            foreach (var fullPath in _tileProcessingSetting.Process.Keys)
            {
                _logger.LogInformation($"Starting processing file '{fullPath}'");

                if (!TileProcess.IsGeoJsonFile(fullPath) && !TileProcess.IsMbTileFile(fullPath))
                {
                    throw new Exception($"The {fullPath} is invalid, only supports .geojson and .mbtiles files.");
                }

                var (username, password, uri) = _fileServerSetting;
                await TileProcess.DownloadFile(username, password, uri, $"/{fullPath}");

                try
                {
                    var fileNameVectorTiles = string.Empty;

                    if (TileProcess.IsGeoJsonFile(fullPath))
                    {
                        var tippeCannoeArgs = _tileProcessingSetting.Process[fullPath];
                        TileProcess.RunTippecanoe(tippeCannoeArgs);
                        fileNameVectorTiles = TileProcess.ChangeFileExtensionName(fullPath, ".mbtiles");
                    }
                    else if (TileProcess.IsMbTileFile(fullPath))
                    {
                        fileNameVectorTiles = fullPath;
                    }
                    else
                    {
                        throw new Exception($"Could not handle file extension of type: {Path.GetExtension(fullPath)}");
                    }

                    File.Move($"/tmp/{fileNameVectorTiles}", $"{_tileProcessingSetting.Destination}/{fileNameVectorTiles}", true);

                    TileProcess.ReloadMbTileServer();
                    _logger.LogInformation($"Finished processing {fullPath}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message + "\n" + ex.StackTrace);
                }
            }
        }
    }
}
