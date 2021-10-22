using System;
using Topos.Config;
using Topos.Serialization;

namespace TileWatcher.Serialize
{
    public static class SerializationExtension
    {
        public static void FileNotificationSerializer(this StandardConfigurer<IMessageSerializer> configurer)
        {
            if (configurer == null)
                throw new ArgumentNullException(nameof(configurer));

            StandardConfigurer.Open(configurer)
                .Register(c => new FileNotificationSerializer());
        }
    }
}
