namespace WSFBackendApi.DTOs;

public class LocationCreateDto
{
    public required string Name { get; set; }
    public required string Description { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public required string Address { get; set; }
}

public class LocationResponseDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public required string Address { get; set; }
    public bool IsActive { get; set; }
    public bool IsVerified { get; set; }
    public Guid UserId { get; set; }
    public required string UserFullName { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class LocationFilterType
{
    public Guid? UserId { get; set; }
    public bool? IsVerified { get; set; }
    public bool? IsActive { get; set; }
    public string UserRole { get; set; } = "User";
}