using System.Text.Json.Serialization;

namespace EventManagementApi.Entity
{
    public class EventImage
    {
        public Guid Id { get; set; }
        public Guid EventId { get; set; }
        public string? Url { get; set; }
        [JsonIgnore]
        public virtual Event? Event { get; set; }
    }
}