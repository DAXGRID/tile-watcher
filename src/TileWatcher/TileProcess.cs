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
            var pId = RetrieveProcessId("mbtileserver");
            var startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = true;
            startInfo.UseShellExecute = false;
            startInfo.FileName = "kill";
            startInfo.Arguments = $"-HUP {pId}";
            startInfo.RedirectStandardOutput = true;

            var stdout = "";
            using (var process = Process.Start(startInfo))
            {
                stdout = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
            }

            return stdout;
        }

        private static string RetrieveProcessId(string processName)
        {
            var startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = true;
            startInfo.UseShellExecute = false;
            startInfo.FileName = "pgrep";
            startInfo.Arguments = processName;
            startInfo.RedirectStandardOutput = true;

            var processId = "";

            using (var process = Process.Start(startInfo))
            {
                processId = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
            }

            return processId;
        }
    }
}
