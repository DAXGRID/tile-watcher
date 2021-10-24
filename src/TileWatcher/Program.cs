using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using TileWatcher.Config;

namespace TileWatcher
{
    internal class program
    {
        async static Task Main(string[] args)
        {
            var root = Directory.GetCurrentDirectory();
            var dotenv = Path.Combine(root, ".env");
            DotEnv.Load(dotenv);

            using (var host = HostConfig.Configure())
            {
                await host.StartAsync();
                await host.WaitForShutdownAsync();
            }
        }
    }
}
