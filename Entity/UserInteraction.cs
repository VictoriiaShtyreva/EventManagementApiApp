namespace EventManagementApi.Entity
{
    public class UserInteraction
    {
        public string? Id { get; set; }
        public string? EventId { get; set; } // Foreign key to Event in PostgreSQL
        public string? InteractionType { get; set; }
        public string? UserId { get; set; } // Foreign key to User from EntraID

    }
}