namespace TileWatcher.Config
{
    internal record FileServerSetting
    {
        public string Username { get; init; }
        public string Password { get; init; }
        public string Uri { get; init; }

        internal void Deconstruct(out string username, out string password, out string uri)
        {
            username = Username;
            password = Password;
            uri = Uri;
        }
    }
}
