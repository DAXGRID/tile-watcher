using System;

namespace TileWatcher
{
    internal interface IFileNotificationConsumer : IDisposable
    {
        void Start();
    }
}
