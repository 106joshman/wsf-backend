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
    public string UserId { get; set; }
    public string? Username { get; set; }
}