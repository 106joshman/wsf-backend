using Microsoft.EntityFrameworkCore;
using WSFBackendApi.Data;
using WSFBackendApi.Models;


namespace WSFBackendApi.Seeders;

public static class SuperAdminSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var superAdminEmail = "superadmin@wsf.com";
        var exists = await context.Admins.AnyAsync(u => u.Email == superAdminEmail);

        if (exists)
        {
            Console.WriteLine("Super admin already exists");
            return;
        }

        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        var passwordText = config["AdminSettings:superAdminPassword"];
        if (string.IsNullOrEmpty(passwordText))
        {
            Console.WriteLine("WARNING: AdminSettings:superAdminPassword not set. Skipping super admin creation.");
            return;
        }

        var superAdmin = new AdminUser
        {
            Id = Guid.NewGuid(),
            Email = superAdminEmail,
            First_name = "Super",
            Last_name = "Admin",
            PhoneNumber = "",
            AvatarUrl = null,
            Password =  BCrypt.Net.BCrypt.HashPassword(passwordText),
            Role = "super_admin",
            CreatedAt = DateTime.UtcNow,
            LastLogin = DateTime.UtcNow
        };

        context.Admins.Add(superAdmin);
        await context.SaveChangesAsync();
    }
}