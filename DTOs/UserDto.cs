namespace WSFBackendApi.DTOs;

public class UserUpdateDto
{
    public required string First_name { get; set; }
    public required string Last_name { get; set; }
    public required string PhoneNumber { get; set; }
    public required string AvatarUrl { get; set; }
}

public class UserProfileResponseDto
{
    public Guid Id { get; set; }
    public required string Email { get; set; }
    public required string First_name { get; set; }
    public required string Last_name { get; set; }
    public string? PhoneNumber { get; set; }
    public string? AvatarUrl { get; set; }
    public required string Role { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLogin { get; set; }
}

public class ChangePasswordDto
{
    public required string CurrentPassword { get; set; }
    public required string NewPassword { get; set; }
}