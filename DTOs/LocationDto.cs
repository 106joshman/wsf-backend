namespace WSFBackendApi.DTOs;

public class LocationCreateDto
{
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required string Contact { get; set; }
    public string? Country { get; set; }
    public string? State { get; set; }
    public string? LGA { get; set; }
    public required string Address { get; set; }
    public required string District { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}

public class LocationResponseDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required string Contact { get; set; }
    public required string District { get; set; }
    public string? Country { get; set; }
    public string? State { get; set; }
    public string? LGA { get; set; }
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
    public string? UserRole { get; set; }

    // New filtering parameters
    public string? District { get; set; }
    public string? Address { get; set; }
    public string? Country { get; set; }
    public string? State { get; set; }
    public string? LGA { get; set; }
    public string? Name { get; set; }

    // Date range filtering
    public DateTime? CreatedFrom { get; set; }
    public DateTime? CreatedTo { get; set; }

    // General search term
    public string? SearchTerm { get; set; }
}

public class UpdateLocationDto
{
    public  string? Name { get; set; }
    public  string? Description { get; set; }
    public  string? Contact { get; set; }
    public  string? District { get; set; }
    public  string? Address { get; set; }
    public required string State { get; set; }
    public required string LGA { get; set; }
}