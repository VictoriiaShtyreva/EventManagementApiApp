namespace EventManagementApi.DTO
{
    public class EventCreateDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Location { get; set; }
        public DateTime Date { get; set; }
        public string? OrganizerId { get; set; } // EntraID = UserID
    }

    public class EventUpdateDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Location { get; set; }
        public DateTime Date { get; set; }
    }

    public class EventRegistrationDto
    {
        public Guid? EventId { get; set; }
        public string? UserId { get; set; }
    }
}