namespace EventManagementApi.Entity
{
    public class EventImage
    {
        public Guid Id { get; set; }
        public Guid EventId { get; set; }
        public string? Url { get; set; }
        public virtual Event? Event { get; set; }
    }
}