namespace EventManagementApi.DTO
{
    public class EventCreateDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Location { get; set; }
        public string Date { get; set; } = DateTime.UtcNow.ToString();
        public string? OrganizerId { get; set; } // EntraID = UserID
        public string? Type { get; set; }
        public string? Category { get; set; }

    }

    public class EventUpdateDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Location { get; set; }
        public string Date { get; set; } = DateTime.UtcNow.ToString();
    }

}