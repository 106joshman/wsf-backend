using System.ComponentModel.DataAnnotations;

namespace WSFBackendApi.DTOs;

public class RegisterDto
{
    [Required]
    public required string First_name {get; set;}

    [Required]
    public required string Last_name {get; set;}

    [Required]
    [EmailAddress]
    public required string Email { get; set; }

    [Required]
    [DataType(DataType.Password)]
    public required string Password { get; set; }
}

public class LoginDto
{
    [Required]
    [EmailAddress]
    public required string Email { get; set; }

    [Required]
    [DataType(DataType.Password)]
    public required string Password { get; set; }
}

public class AuthResponseDto
{
    public required string Token { get; set; }
    public string? RefreshToken { get; set; }
    public Guid UserId { get; set; }
    public string? First_name { get; set; }
    public string? Last_name { get; set; }
    public string? PhoneNumber { get; set; }
    public string? AvatarUrl { get; set; }
    public string? Email { get; set; }
    public required string Role { get; set; } = "User";
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLogin { get; set; }
}