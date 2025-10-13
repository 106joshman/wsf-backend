using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace WSFBackendApi.Models;

[Index(nameof(Email), IsUnique = true)]
[Index(nameof(Role))]
public class User
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [EmailAddress]
    public required string Email { get; set; }

    [Required]
    public required string First_name { get; set; }

    [Required]
    public required string Last_name { get; set; }

    [Required]
    [DataType(DataType.Password)]
    public required string Password { get; set; }

    public string? PhoneNumber { get; set; }

    public string? AvatarUrl { get; set; }

    public required string Role { get; set; } = "user";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;

    public DateTime? LastLogin { get; set; }

    // NAVIGATION PROPERTY FOR LOCATIONS
    public ICollection<Location>? Locations { get; set; }

    internal static Guid FindFirst(string nameIdentifier)
    {
        throw new NotImplementedException();
    }
}
