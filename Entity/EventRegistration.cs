namespace EventManagementApi.Entity
{
    public class EventRegistration
    {
        public Guid EventId { get; set; }
        public string? UserId { get; set; } // EntraID = UserID
        public string? Action { get; set; } = string.Empty;
        public virtual Event? Event { get; set; }
    }
}