namespace EventManagementApi.Entity
{
    public class EventMetadata
    {
        public string? Id { get; set; }
        public string? Type { get; set; }
        public string? Category { get; set; }
        public string? EventId { get; set; } // Foreign key to Event in PostgreSQL
    }
}