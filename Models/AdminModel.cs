using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WSFBackendApi.Models;

public class AdminUser
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

    public string? Country { get; set; }

    public string? State { get; set; }

    public string? Address { get; set; }

    public required string Role { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = false;

    public DateTime? LastLogin { get; set; }

}