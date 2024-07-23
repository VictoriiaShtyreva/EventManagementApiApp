namespace EventManagementApi.Entity
{
    public class Event
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Location { get; set; }
        public string? Date { get; set; }
        public string? OrganizerId { get; set; } // EntraID = UserID

        public virtual ICollection<EventImage> EventImages { get; set; } = new List<EventImage>();
        public virtual ICollection<EventDocument> EventDocuments { get; set; } = new List<EventDocument>();
    }

}