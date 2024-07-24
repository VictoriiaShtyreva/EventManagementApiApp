using Newtonsoft.Json;

namespace EventManagementApi.Entity
{
    public class EventMetadata
    {
        [JsonProperty("id")]
        public string? Id { get; set; }
        [JsonProperty("Type")]
        public string? Type { get; set; }
        [JsonProperty("Category")]
        public string? Category { get; set; }
        [JsonProperty("eventId")]
        public string? EventId { get; set; } // PartitionKey
    }
}