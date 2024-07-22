using System.ComponentModel.DataAnnotations;

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
    }
}