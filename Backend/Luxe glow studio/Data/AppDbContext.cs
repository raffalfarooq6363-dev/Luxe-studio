using Microsoft.EntityFrameworkCore;
using Luxe_glow_studio.Models;

namespace Luxe_glow_studio.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Core entities
        public DbSet<User> Users { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<ServiceCategory> ServiceCategories { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<ReviewVote> ReviewVotes { get; set; }

        // Practitioner management
        public DbSet<Practitioner> Practitioners { get; set; }
        public DbSet<PractitionerService> PractitionerServices { get; set; }
        public DbSet<PractitionerAvailability> PractitionerAvailabilities { get; set; }

        // Offers and promotions
        public DbSet<OfferCode> OfferCodes { get; set; }
        public DbSet<OfferCodeUsage> OfferCodeUsages { get; set; }
        public DbSet<Promotion> Promotions { get; set; }

        // Notifications
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<NotificationTemplate> NotificationTemplates { get; set; }
        public DbSet<NotificationPreference> NotificationPreferences { get; set; }

        // Gallery and portfolio
        public DbSet<GalleryImage> GalleryImages { get; set; }
        public DbSet<GalleryImageLike> GalleryImageLikes { get; set; }
        public DbSet<Portfolio> Portfolios { get; set; }

        // System management
        public DbSet<BusinessSetting> BusinessSettings { get; set; }
        public DbSet<BusinessHour> BusinessHours { get; set; }
        public DbSet<Holiday> Holidays { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        // Customer engagement
        public DbSet<ContactInquiry> ContactInquiries { get; set; }
        public DbSet<LoyaltyPoint> LoyaltyPoints { get; set; }
        public DbSet<Referral> Referrals { get; set; }
        public DbSet<FAQ> FAQs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User entity configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.PhoneNumber);
            });

            // Service category relationship
            modelBuilder.Entity<Service>(entity =>
            {
                entity.HasOne(s => s.Category)
                      .WithMany(c => c.Services)
                      .HasForeignKey(s => s.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Practitioner relationships
            modelBuilder.Entity<Practitioner>(entity =>
            {
                entity.HasOne(p => p.User)
                      .WithOne(u => u.PractitionerProfile)
                      .HasForeignKey<Practitioner>(p => p.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Practitioner service relationship
            modelBuilder.Entity<PractitionerService>(entity =>
            {
                entity.HasOne(ps => ps.Practitioner)
                      .WithMany(p => p.PractitionerServices)
                      .HasForeignKey(ps => ps.PractitiOnerId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ps => ps.Service)
                      .WithMany(s => s.PractitionerServices)
                      .HasForeignKey(ps => ps.ServiceId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Appointment relationships
            modelBuilder.Entity<Appointment>(entity =>
            {
                entity.HasOne(a => a.User)
                      .WithMany(u => u.Appointments)
                      .HasForeignKey(a => a.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.Service)
                      .WithMany(s => s.Appointments)
                      .HasForeignKey(a => a.ServiceId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Payment relationships
            modelBuilder.Entity<Payment>(entity =>
            {
                entity.HasOne(p => p.User)
                      .WithMany(u => u.Payments)
                      .HasForeignKey(p => p.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(p => p.Appointment)
                      .WithMany()
                      .HasForeignKey(p => p.AppointmentId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Review relationships
            modelBuilder.Entity<Review>(entity =>
            {
                entity.HasOne(r => r.User)
                      .WithMany(u => u.Reviews)
                      .HasForeignKey(r => r.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(r => r.Service)
                      .WithMany(s => s.Reviews)
                      .HasForeignKey(r => r.ServiceId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(r => r.Practitioner)
                      .WithMany(p => p.PractitionerReviews)
                      .HasForeignKey(r => r.PractitionerId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // Offer code usage
            modelBuilder.Entity<OfferCodeUsage>(entity =>
            {
                entity.HasOne(ocu => ocu.OfferCode)
                      .WithMany(oc => oc.Usages)
                      .HasForeignKey(ocu => ocu.OfferCodeId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Notification relationships
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasOne(n => n.User)
                      .WithMany()
                      .HasForeignKey(n => n.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Gallery image likes
            modelBuilder.Entity<GalleryImageLike>(entity =>
            {
                entity.HasIndex(e => new { e.GalleryImageId, e.UserId }).IsUnique();
            });

            // Loyalty points
            modelBuilder.Entity<LoyaltyPoint>(entity =>
            {
                entity.HasOne(lp => lp.User)
                      .WithMany()
                      .HasForeignKey(lp => lp.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Referral system
            modelBuilder.Entity<Referral>(entity =>
            {
                entity.HasOne(r => r.ReferrerUser)
                      .WithMany()
                      .HasForeignKey(r => r.ReferrerUserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(r => r.ReferredUser)
                      .WithMany()
                      .HasForeignKey(r => r.ReferredUserId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasIndex(e => e.ReferralCode).IsUnique();
            });

            // Business settings
            modelBuilder.Entity<BusinessSetting>(entity =>
            {
                entity.HasIndex(e => e.Key).IsUnique();
            });

            // Configure decimal precision
            modelBuilder.Entity<Service>()
                .Property(s => s.Price)
                .HasPrecision(10, 2);

            modelBuilder.Entity<Payment>()
                .Property(p => p.TotalAmount)
                .HasPrecision(10, 2);

            modelBuilder.Entity<Practitioner>()
                .Property(p => p.HourlyRate)
                .HasPrecision(8, 2);

            // Seed data will be added in a separate seeder
        }
    }
}