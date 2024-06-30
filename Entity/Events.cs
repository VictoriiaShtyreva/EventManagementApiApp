using Microsoft.Kiota.Abstractions;

namespace EventManagementApi.Entity
{
    public class Events
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Location { get; set; }
        public DateTime? Date { get; set; }
        public string? OrganizerId { get; set; } // EntraID = UserID
    }
}