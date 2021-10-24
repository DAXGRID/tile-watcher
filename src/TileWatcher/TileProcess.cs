using System;
using System.Diagnostics;

namespace TileWatcher
{
    internal static class TileProcess
    {
        public static void RunTippecanoe(string arguments)
        {
            var startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = false;
            startInfo.UseShellExecute = true;
            startInfo.FileName = "tippecanoe";
            startInfo.Arguments = arguments;

            using (var process = Process.Start(startInfo))
            {
                if (process is null)
                    throw new Exception("Process tippecanoe cannot be null");

                process.WaitForExit();
            }
        }

        public static void SendReloadSignal(int pId)
        {
            var startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = false;
            startInfo.UseShellExecute = true;
            startInfo.FileName = "kill";
            startInfo.Arguments = $"-HUP {pId}";

            using (var process = Process.Start(startInfo))
            {
                if (process is null)
                    throw new Exception("Process kill cannot be null");

                process.WaitForExit();
            }
        }
    }
}
