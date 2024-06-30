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
        public DbSet<Events> Events { get; set; }
        public DbSet<EventRegistrations> EventRegistrations { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Composite key configuration for EventRegistration
            builder.Entity<EventRegistrations>()
                .HasKey(er => new { er.EventId, er.UserId });

            // Adding an index to Event Name for faster searches
            builder.Entity<Events>()
                .HasIndex(e => e.Name);

            // One-to-many relationship between Event and EventRegistration
            builder.Entity<EventRegistrations>()
                .HasOne<Events>()
                .WithMany()
                .HasForeignKey(er => er.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            // Default values and constraints
            builder.Entity<Events>()
                .Property(e => e.Date)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

        }
    }
}
