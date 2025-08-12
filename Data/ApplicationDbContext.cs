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
        public DbSet<AdminUser> Admin { get; set; }
        public DbSet<Location> Locations { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // CONFIGURE USER ENTITY
            // ENSURE EMAIL IS UNIQUE IN THE DB
            modelBuilder.Entity<User>().ToTable("Users")
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<Location>()
                .HasOne(l => l.User)
                .WithMany(u => u.Locations)
                .HasForeignKey(l => l.UserId)
                .IsRequired();
        }
    }
}