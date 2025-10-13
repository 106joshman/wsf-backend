using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using WSFBackendApi.Data;
using WSFBackendApi.DTOs;
using WSFBackendApi.Models;

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
            throw new KeyNotFoundException("User not found");
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

        // SAVE UPDATE MADE
        _context.Users.Update(user);
        // UPDATE CHANGES TO THE DATABASE
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
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            throw new Exception("User not found");
        }

        if (!BCrypt.Net.BCrypt.Verify(changePasswordDto.CurrentPassword, user.Password))
        {
            throw new Exception("Current password is incorrect!");
        }

        if (string.IsNullOrWhiteSpace(changePasswordDto.NewPassword) || changePasswordDto.NewPassword.Length < 8)
        {
            throw new Exception("New password must be at least 8 characters long!");
        }

        user.Password = BCrypt.Net.BCrypt.HashPassword(changePasswordDto.NewPassword);

        await _context.SaveChangesAsync();
    }

    public async Task<PaginatedResponse<AdminViewResponseDto>> GetAllAdmins(PaginationParams paginationParams, string? First_name = null, string? Last_name = null, string? email = null, string? state = null)
    {
        var query = _context.Admins.AsQueryable();

        if (!string.IsNullOrWhiteSpace(First_name))
        {
            query = query.Where(u => u.First_name.Contains(First_name));
        }

        if (!string.IsNullOrWhiteSpace(Last_name))
        {
            query = query.Where(u => u.Last_name.Contains(Last_name));
        }

        if (!string.IsNullOrWhiteSpace(email))
        {
            query = query.Where(u => u.Email.Contains(email));
        }

        if (!string.IsNullOrWhiteSpace(state))
        {
            query = query.Where(u => u.State != null && u.State.Contains(state));
        }

        var totalCount = await query.CountAsync();

        var users = await query
            .OrderBy(u => u.First_name)
            .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
            .Take(paginationParams.PageSize)
            .Select(u => new AdminViewResponseDto
            {
                Id = u.Id,
                Email = u.Email,
                First_name = u.First_name,
                Last_name = u.Last_name,
                PhoneNumber = u.PhoneNumber,
                AvatarUrl = u.AvatarUrl,
                State = u.State, // Added missing State field
                Country = u.Country, // Added missing Country field
                Address = u.Address, // Added missing Address field
                Role = u.Role,
                CreatedAt = u.CreatedAt,
                LastLogin = u.LastLogin,
                IsActive = u.IsActive,
            })
            .ToListAsync();

        return new PaginatedResponse<AdminViewResponseDto>
        {
            Items = users,
            TotalCount = totalCount,
            PageNumber = paginationParams.PageNumber,
            PageSize = paginationParams.PageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)paginationParams.PageSize)
        };
    }

    public async Task<PaginatedResponse<UserProfileResponseDto>> GetAllUsers(PaginationParams paginationParams, string? First_name = null, string? Last_name = null, string? email = null)
    {
        var query = _context.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(First_name))
        {
            query = query.Where(u => u.First_name.Contains(First_name));
        }

        if (!string.IsNullOrWhiteSpace(Last_name))
        {
            query = query.Where(u => u.Last_name.Contains(Last_name));
        }

        if (!string.IsNullOrWhiteSpace(email))
        {
            query = query.Where(u => u.Email.Contains(email));
        }

        var totalCount = await query.CountAsync();

        var users = await query
            .OrderBy(u => u.First_name)
            .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
            .Take(paginationParams.PageSize)
            .Select(u => new UserProfileResponseDto
            {
                Id = u.Id,
                Email = u.Email,
                First_name = u.First_name,
                Last_name = u.Last_name,
                PhoneNumber = u.PhoneNumber,
                AvatarUrl = u.AvatarUrl,
                Role = u.Role,
                CreatedAt = u.CreatedAt,
                LastLogin = u.LastLogin
            })
            .ToListAsync();

        return new PaginatedResponse<UserProfileResponseDto>
        {
            Items = users,
            TotalCount = totalCount,
            PageNumber = paginationParams.PageNumber,
            PageSize = paginationParams.PageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)paginationParams.PageSize)
        };
    }
}