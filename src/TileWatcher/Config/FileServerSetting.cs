namespace TileWatcher.Config
{
    internal record FileSeverSetting
    {
        public string Username { get; init; }
        public string Password { get; init; }
        public string Uri { get; init; }
    }
}
