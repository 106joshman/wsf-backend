using Microsoft.EntityFrameworkCore;
using WSFBackendApi.Data;
using WSFBackendApi.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace WSFBackendApi.Seeders
{
    public static class MultipleAdminsSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Roles pool
            var roles = new[] { "Admin", "state_admin", "zonal_admin" };
            var random = new Random();

            // Check if there are already 50 or more admins
            var existingAdmins = await context.Admin
                .Where(u => roles.Contains(u.Role))
                .CountAsync();

            if (existingAdmins >= 50)
            {
                Console.WriteLine("50 or more admin accounts already exist.");
                return;
            }

            var adminsToAdd = 50 - existingAdmins;
            var passwordHash = BCrypt.Net.BCrypt.HashPassword("`080_Password,@1234`");

            for (int i = 0; i < adminsToAdd; i++)
            {
                var role = roles[random.Next(roles.Length)];
                var admin = new AdminUser
                {
                    Id = Guid.NewGuid(),
                    Email = $"admin{i + 1}@wsf.com",
                    First_name = $"AdminFirst{i + 1}",
                    Last_name = $"AdminLast{i + 1}",
                    PhoneNumber = $"070{random.Next(10000000, 99999999)}",
                    AvatarUrl = null,
                    Password = passwordHash,
                    Role = role,
                    CreatedAt = DateTime.UtcNow,
                    LastLogin = null
                };

                context.Admin.Add(admin);
            }

            await context.SaveChangesAsync();
            Console.WriteLine($"{adminsToAdd} admin accounts seeded successfully.");
        }
    }
}
