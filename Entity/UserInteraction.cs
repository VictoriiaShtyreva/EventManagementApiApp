using Newtonsoft.Json;

namespace EventManagementApi.Entity
{
    public class UserInteraction
    {
        [JsonProperty("id")]
        public string? Id { get; set; }
        [JsonProperty("eventId")]
        public string? EventId { get; set; } // PartitionKey
        [JsonProperty("InteractionType")]
        public string? InteractionType { get; set; }
        [JsonProperty("UserId")]
        public string? UserId { get; set; } // Foreign key to User from EntraID

    }
}