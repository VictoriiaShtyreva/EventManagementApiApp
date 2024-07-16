using EventManagementApi.Entity;
using Microsoft.EntityFrameworkCore;

namespace EventManagementApi.Entities
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Event> Events { get; set; }
        public DbSet<EventRegistration> EventRegistrations { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Composite key configuration for EventRegistration
            builder.Entity<EventRegistration>()
                .HasKey(er => new { er.EventId, er.UserId });

            // Adding an index to Event Name for faster searches
            builder.Entity<Event>()
                .HasIndex(e => e.Name);

            // One-to-many relationship between Event and EventRegistration
            builder.Entity<EventRegistration>()
                .HasOne<Event>()
                .WithMany()
                .HasForeignKey(er => er.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            // Default values and constraints
            builder.Entity<Event>()
                .Property(e => e.Date)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

        }
    }
}
