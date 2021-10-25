using System.Diagnostics;

namespace TileWatcher
{
    internal static class TileProcess
    {
        public static string RunTippecanoe(string arguments)
        {
            var startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = true;
            startInfo.UseShellExecute = false;
            startInfo.FileName = "tippecanoe";
            startInfo.Arguments = arguments;
            startInfo.RedirectStandardOutput = true;

            var stdout = "";
            using (var process = Process.Start(startInfo))
            {
                stdout = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
            }

            return stdout;
        }

        public static string ReloadMbTileServer()
        {
            var pIds = RetrieveProcessIds("mbtileserver");

            var startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = true;
            startInfo.UseShellExecute = false;
            startInfo.FileName = "kill";
            startInfo.Arguments = $"-HUP {pIds}";
            startInfo.RedirectStandardOutput = true;

            var stdout = "";
            using (var process = Process.Start(startInfo))
            {
                stdout = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
            }

            return stdout;
        }

        private static string RetrieveProcessIds(string processName)
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

            return string.Join(' ', processIds.Split(','));
        }
    }
}
