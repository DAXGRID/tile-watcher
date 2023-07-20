using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TileWatcher
{
    internal static class TileProcess
    {
        public static bool IsGeoJsonFile(string path)
        {
            return Path.GetExtension(path) == ".geojson";
        }

        public static bool IsMbTileFile(string path)
        {
            return Path.GetExtension(path) == ".mbtiles";
        }

        public static string ChangeFileExtensionName(string filePath, string extension)
        {
            return $"{Path.GetFileNameWithoutExtension(filePath)}{extension}";
        }

        public static async Task DownloadFile(string username, string password, string basePath, string fullPath)
        {
            var token = BasicAuthToken(username, password);
            var fileName = Path.GetFileName(fullPath);
            using var webClient = new WebClient();
            webClient.Headers.Add("Authorization", $"Basic {token}");
            var downloadAddress = new Uri($"{basePath}{fullPath}");
            await webClient.DownloadFileTaskAsync(downloadAddress, $"/tmp/{fileName}");
        }

        public static void RunTippecanoe(string arguments)
        {
            var startInfo = new ProcessStartInfo();
            startInfo.FileName = "tippecanoe";
            startInfo.Arguments = arguments;

            using (var process = Process.Start(startInfo))
            {
                process.WaitForExit();
            }
        }

        public static void ReloadMbTileServer()
        {
            var processIds = RetrieveProcessIds("mbtileserver");
            if (processIds.Count == 0)
                return;

            var startInfo = new ProcessStartInfo();
            startInfo.FileName = "kill";
            // We use the first processId since it spawned all sub processes.
            startInfo.Arguments = $"-HUP {processIds.First()}";

            using (var process = Process.Start(startInfo))
            {
                process.WaitForExit();
            }
        }

        private static List<int> RetrieveProcessIds(string processName)
        {
            var startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = true;
            startInfo.UseShellExecute = false;
            startInfo.FileName = "pgrep";
            startInfo.Arguments = $"{processName} -d ,";
            startInfo.RedirectStandardOutput = true;

            var processIds = "";
            using (var process = Process.Start(startInfo))
            {
                processIds = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
            }

            if (!processIds.Contains(','))
                return new List<int>();

            return ConvertProcessIdOutput(processIds);
        }

        private static List<int> ConvertProcessIdOutput(string processIds)
        {
            return processIds.Split(',').Select(x => Convert.ToInt32(x)).OrderBy(x => x).ToList();
        }

        private static string BasicAuthToken(string username, string password)
        {
            return Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes($"{username}:{password}"));
        }
    }
}
