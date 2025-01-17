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
        public DbSet<EventImage> EventImages { get; set; }
        public DbSet<EventDocument> EventDocuments { get; set; }

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
           .HasOne(er => er.Event)
           .WithMany(e => e.EventRegistrations)
           .HasForeignKey(er => er.EventId)
           .OnDelete(DeleteBehavior.Cascade);

            // Default values and constraints
            builder.Entity<Event>()
                .Property(e => e.Date)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // One-to-many relationship between Event and EventImage
            builder.Entity<EventImage>()
                .HasOne(ei => ei.Event)
                .WithMany(e => e.EventImages)
                .HasForeignKey(ei => ei.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            // One-to-many relationship between Event and EventDocument
            builder.Entity<EventDocument>()
                .HasOne(ed => ed.Event)
                .WithMany(e => e.EventDocuments)
                .HasForeignKey(ed => ed.EventId)
                .OnDelete(DeleteBehavior.Cascade);

        }
    }
}
