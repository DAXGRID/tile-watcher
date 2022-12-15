using System;
using System.Text.Json.Serialization;

namespace TileWatcher
{
    public record FileChangedEvent
    {
        [JsonPropertyName("eventId")]
        public Guid EventId { get; init; }

        [JsonPropertyName("eventType")]
        public string EventType { get; init; }

        [JsonPropertyName("eventTimeStamp")]
        public DateTime EventTimeStamp { get; init; }

        [JsonPropertyName("fullPath")]
        public string FullPath { get; init; }

        [JsonPropertyName("sha256CheckSum")]
        public string Sha256CheckSum { get; init; }

        [JsonConstructor]
        public FileChangedEvent(
            Guid eventId,
            string eventType,
            DateTime eventTimeStamp,
            string fullPath,
            string sha256CheckSum)
        {
            EventId = eventId;
            EventType = eventType;
            EventTimeStamp = eventTimeStamp;
            FullPath = fullPath;
            Sha256CheckSum = sha256CheckSum;
        }
    }
}
