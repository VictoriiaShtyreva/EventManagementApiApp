namespace EventManagementApi.Entity
{
    public class EventRegistration
    {
        public string? EventId { get; set; }
        public string? UserId { get; set; } // EntraID = UserID
        public string? Action { get; set; } = string.Empty;
    }
}