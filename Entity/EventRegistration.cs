namespace EventManagementApi.Entity
{
    public class EventRegistration
    {
        public string? EventId { get; set; }
        public string? UserId { get; set; } // EntraID = UserID
        public DateTime RegistrationDate { get; set; }
    }
}