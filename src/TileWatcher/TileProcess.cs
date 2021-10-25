using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

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
            var joinedProcessIds = JoinProcessIds(pIds);

            var startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = true;
            startInfo.UseShellExecute = false;
            startInfo.FileName = "kill";
            startInfo.Arguments = $"-HUP {joinedProcessIds}";
            startInfo.RedirectStandardOutput = true;

            var stdout = "";
            using (var process = Process.Start(startInfo))
            {
                stdout = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
            }

            return stdout;
        }

        private static List<int> RetrieveProcessIds(string processName)
        {
            var startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = true;
            startInfo.UseShellExecute = false;
            startInfo.FileName = "pgrep";
            startInfo.Arguments = processName;
            startInfo.RedirectStandardOutput = true;

            var processIds = "";

            using (var process = Process.Start(startInfo))
            {
                processIds = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
            }

            return processIds.Split(' ').Select(x => Convert.ToInt32(x)).ToList();
        }

        private static string JoinProcessIds(List<int> pIds)
        {
            return string.Join(" ", pIds.Select(x => $"'{x}'"));
        }
    }
}
