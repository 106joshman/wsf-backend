using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using WSFBackendApi.Data;
using WSFBackendApi.DTOs;

namespace WSFBackendApi.Services;
public class UserService
{
    private readonly ApplicationDbContext _context;

    public UserService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<UserProfileResponseDto> UpdateUserProfile(Guid userId, UserUpdateDto updateDto)
    {
        // FIND USER IN DATABASE
        Console.WriteLine($"Received update request for {updateDto.First_name}");
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            throw new Exception("User not found");
        }

        // Update fields if they are not null in the DTO
        if (!string.IsNullOrWhiteSpace(updateDto.First_name))
            user.First_name = updateDto.First_name;

        if (!string.IsNullOrWhiteSpace(updateDto.Last_name))
            user.Last_name = updateDto.Last_name;

        if (!string.IsNullOrWhiteSpace(updateDto.PhoneNumber))
            user.PhoneNumber = updateDto.PhoneNumber;

        if (!string.IsNullOrWhiteSpace(updateDto.AvatarUrl))
            user.AvatarUrl = updateDto.AvatarUrl;

        // Save changes
        await _context.SaveChangesAsync();

        // Map to response DTO
        return new UserProfileResponseDto
        {
            Id = user.Id,
            Email = user.Email,
            First_name = user.First_name,
            Last_name = user.Last_name,
            PhoneNumber = user.PhoneNumber,
            AvatarUrl = user.AvatarUrl,
            Role = user.Role,
            CreatedAt = user.CreatedAt,
            LastLogin = user.LastLogin
        };
    }

    // Optional: Method to get user profile
    public async Task<UserProfileResponseDto> GetUserProfile(Guid userId)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            throw new Exception("User not found");
        }

        return new UserProfileResponseDto
        {
            Id = user.Id,
            Email = user.Email,
            First_name = user.First_name,
            Last_name = user.Last_name,
            PhoneNumber = user.PhoneNumber,
            AvatarUrl = user.AvatarUrl,
            Role = user.Role,
            CreatedAt = user.CreatedAt,
            LastLogin = user.LastLogin
        };
    }

    public async Task ChangePassword(Guid userId, ChangePasswordDto changePasswordDto)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id ==userId);

            if (user == null)
            {
                throw new Exception ("User not found");
            }

            if (!BCrypt.Net.BCrypt.Verify(changePasswordDto.CurrentPassword, user.Password))
            {
                throw new Exception("Current password is incorrect!");
            }

            if (string.IsNullOrWhiteSpace(changePasswordDto.NewPassword) || changePasswordDto.NewPassword.Length < 8)
            {
                throw new Exception ("New password must be at least 8 characters long!");
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(changePasswordDto.NewPassword);

            await _context.SaveChangesAsync();
    }
}