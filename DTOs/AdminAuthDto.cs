using System.ComponentModel.DataAnnotations;

namespace WSFBackendApi.DTOs
{
    public class AdminRegisterDto
    {
        [Required]
        public required string First_name { get; set; }

        [Required]
        public required string Last_name { get; set; }

        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        [Required]
        public required string PhoneNumber { get; set; }

        public required string Country { get; set; }

        public required string State { get; set; }

        public required string Address { get; set; }

        public required string Role { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public required string Password { get; set; }
    }

    // Response DTO for admin creation (limited fields + success message)
    public class AdminCreateResponseDto
    {
        public Guid Id { get; set; }
        public string? First_name { get; set; }
        public string? Last_name { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Role { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Message { get; set; } = "Admin created successfully";
    }

    // Response DTO for admin login (full object with token)
    public class AdminLoginResponseDto
    {
        public Guid Id { get; set; }
        public string? RefreshToken { get; set; }
        public string? First_name { get; set; }
        public string? Last_name { get; set; }
        public string? PhoneNumber { get; set; }
        public string? AvatarUrl { get; set; }
        public string? Email { get; set; }
        public required string Role { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLogin { get; set; }
        public string? State { get; set; }
        public string? Country { get; set; }
        public string? Address { get; set; }
        public string? Token { get; set; }
        public bool IsActive { get; set; }
    }

    // Response DTO for viewing admin details (no token, includes location info)
    public class AdminViewResponseDto
    {
        public Guid Id { get; set; }
        public string? First_name { get; set; }
        public string? AvatarUrl { get; set; }
        public string? Last_name { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? State { get; set; }
        public string? Country { get; set; }
        public string? Address { get; set; }
        public required string Role { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLogin { get; set; }
        public bool IsActive { get; set; }
    }

    public class DeleteAdminsRequest
    {
        [Required]
        public List<Guid> Ids { get; set; } = new List<Guid>();
    }
}