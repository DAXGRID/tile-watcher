using System;
using System.Text;
using Newtonsoft.Json;
using Topos.Serialization;

namespace TileWatcher.Serialize
{
    internal class FileNotificationSerializer : IMessageSerializer
    {
        public ReceivedLogicalMessage Deserialize(ReceivedTransportMessage message)
        {
            if (message is null)
                throw new ArgumentNullException($"{nameof(ReceivedTransportMessage)} is null");

            var messageBody = Encoding.UTF8.GetString(message.Body, 0, message.Body.Length);

            var fileChangedEvent = JsonConvert.DeserializeObject<FileChangedEvent>(messageBody);

            if (fileChangedEvent is null)
                throw new ArgumentNullException($"{nameof(FileChangedEvent)} cannot be null.");

            return new ReceivedLogicalMessage(message.Headers, fileChangedEvent, message.Position);
        }

        public TransportMessage Serialize(LogicalMessage message)
        {
            throw new System.NotImplementedException();
        }
    }
}
