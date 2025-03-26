using System.ComponentModel.DataAnnotations;

namespace WSFBackendApi.Models;

public class Location
{
    [Key]
    public int Id { get; set; }

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

    [StringLength(200)]
    public required string Address { get; set; }

    // FOREIGN KEY FOR USER
    public int UserId { get; set; }

    // NAVIGATION PROPERTY FOR USER
    public required User User { get; set; }

    // ADDITIONAL METADATA
    public bool IsAdminLocation { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}