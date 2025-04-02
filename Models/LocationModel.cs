using System.ComponentModel.DataAnnotations;

namespace WSFBackendApi.Models;

public class Location
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [StringLength(100)]
    public required string Name { get; set; }

    [Required]
    [StringLength(500)]
    public required string Description { get; set; }

    [Required]
    public double Latitude { get; set; }

    [Required]
    public double Longitude { get; set; }

    public bool IsActive { get; set; } = true;

    public bool IsVerified { get; set; } = false;

    [StringLength(200)]
    public required string Address { get; set; }

    // FOREIGN KEY FOR USER
    public Guid UserId { get; set; }

    // NAVIGATION PROPERTY FOR USER
    public required User User { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}