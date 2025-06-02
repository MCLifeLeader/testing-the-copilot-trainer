using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MyChatApp.Web.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
    {
        /// <summary>
        /// Contact relationships between users
        /// </summary>
        public DbSet<Contact> Contacts { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure Contact entity
            builder.Entity<Contact>(entity =>
            {
                // Create unique index to prevent duplicate contact requests
                entity.HasIndex(c => new { c.RequesterId, c.ReceiverId })
                      .IsUnique()
                      .HasDatabaseName("IX_Contacts_RequesterReceiver");

                // Configure relationships
                entity.HasOne(c => c.Requester)
                      .WithMany()
                      .HasForeignKey(c => c.RequesterId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(c => c.Receiver)
                      .WithMany()
                      .HasForeignKey(c => c.ReceiverId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Set default values
                entity.Property(c => c.CreatedAt)
                      .HasDefaultValueSql("GETUTCDATE()");

                entity.Property(c => c.UpdatedAt)
                      .HasDefaultValueSql("GETUTCDATE()");
            });
        }
    }
}