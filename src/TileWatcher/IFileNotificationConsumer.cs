using System.Threading.Tasks;

namespace TileWatcher
{
    internal interface IFileNotificationConsumer
    {
        Task Start();
    }
}
