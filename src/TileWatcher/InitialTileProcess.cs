using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
                try
                {
                    _logger.LogInformation($"Starting processing file '{fullPath}'");

                    if (!TileProcess.IsGeoJsonFile(fullPath))
                        throw new Exception($"The {fullPath} is invalid, only supports .geojson.");

                    var (username, password, uri) = _fileServerSetting;
                    await TileProcess.DownloadFile(username, password, uri, $"/{fullPath}");

                    var tippeCannoeArgs = _tileProcessingSetting.Process[fullPath];
                    TileProcess.RunTippecanoe(tippeCannoeArgs);

                    var fileNameVectorTiles = TileProcess.ChangeFileExtensionName(fullPath, ".mbtiles");
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
