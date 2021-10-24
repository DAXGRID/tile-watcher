using System.Collections.Generic;

namespace TileWatcher.Config
{
    internal record TileProcessSetting
    {
        public Dictionary<string, string> Process { get; init; }
        public string Destination { get; init; }
    }
}
