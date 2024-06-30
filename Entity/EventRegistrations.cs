namespace EventManagementApi.Entity
{
    public class EventRegistrations
    {
        public string? EventId { get; set; }
        public string? UserId { get; set; } // EntraID = UserID
        public DateTime RegistrationDate { get; set; }
    }
}