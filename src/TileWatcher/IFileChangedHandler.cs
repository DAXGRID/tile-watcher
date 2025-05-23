using System;
using System.Threading.Tasks;

namespace TileWatcher
{
    internal interface IFileChangedHandler
    {
        Task Handle(FileChangedEvent fileChangedEvent, Action finishedCallback);
    }
}
