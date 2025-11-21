using Microsoft.EntityFrameworkCore;
using WSFBackendApi.Models;

namespace WSFBackendApi.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // PREDEFINED DATABASE TABLES STRUCTURE
        public DbSet<User> Users { get; set; }

        public DbSet<AdminUser> Admins { get; set; }

        public DbSet<Location> Locations { get; set; }

        public DbSet<Teaching> Teachings { get; set; }

        public DbSet<TeachingWeek> TeachingWeeks { get; set; }

        public DbSet<PrayerOutline> PrayerOutlines { get; set; }

        public DbSet<PrayerPoint> PrayerPoints { get; set; }

        public DbSet<PushNotificationToken> PushNotificationTokens { get; set; }

        public DbSet<HomeCellSelection> HomeCellSelections { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // CONFIGURE USER ENTITY
            // ENSURE EMAIL IS UNIQUE IN THE DB
            modelBuilder.Entity<User>()
                .ToTable("Users")
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<Location>()
                .HasOne(l => l.User)
                .WithMany(u => u.Locations)
                .HasForeignKey(l => l.UserId)
                .IsRequired();

            modelBuilder.Entity<AdminUser>()
                .ToTable("Admin")
                .HasIndex(a => a.Email)
                .IsUnique();

            // TEACHING CONFIGURATION
            modelBuilder.Entity<Teaching>()
                .HasMany(t => t.Weeks)
                .WithOne(w => w.Teaching)
                .HasForeignKey(w => w.TeachingId);

            // PRAYER CONFIGURATION
            modelBuilder.Entity<PrayerOutline>()
                .HasMany(p => p.Schedule)
                .WithOne(s => s.PrayerOutline)
                .HasForeignKey(s => s.PrayerOutlineId);

            modelBuilder.Entity<PrayerOutline>()
                .HasMany(p => p.Prayers)
                .WithOne(pp => pp.PrayerOutline)
                .HasForeignKey(pp => pp.PrayerOutlineId);
        }
    }
}