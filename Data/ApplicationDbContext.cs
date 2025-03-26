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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // CONFIGURE USER ENTITY
            // ENSURE EMAIL IS UNIQUE IN THE DB
            modelBuilder.Entity<User>().ToTable("Users")
                .HasIndex(u => u.Email)
                .IsUnique();

            // ENSURE USER ID IS UNIQUE IN THE DB
            modelBuilder.Entity<User>()
                .Property(u => u.Id)
                .HasDefaultValueSql("NEWID");
        }
    }
}