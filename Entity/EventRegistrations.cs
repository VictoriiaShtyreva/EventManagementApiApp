namespace EventManagementApi.Entity
{
    public class EventRegistrations
    {
        public Guid? EventId { get; set; }
        public string? UserId { get; set; } // EntraID = UserID
        public DateTime RegistrationDate { get; set; }
    }
}