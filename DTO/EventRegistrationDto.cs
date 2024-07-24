namespace EventManagementApi.DTO
{
    public class EventRegistrationDto
    {
        public Guid EventId { get; set; }
        public string? UserId { get; set; }
        public string? Action { get; set; }
    }

    public class EventWithRegistrationCountDto
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Location { get; set; }
        public string? Date { get; set; }
        public int RegisteredUserCount { get; set; }
    }

}