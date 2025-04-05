using Microsoft.EntityFrameworkCore;
using WSFBackendApi.Data;
using WSFBackendApi.DTOs;
using WSFBackendApi.Models;

namespace WSFBackendApi.Services;

public class LocationService
{
    private readonly ApplicationDbContext _context;

    public LocationService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<LocationResponseDto> CreateLocation(Guid UserId, LocationCreateDto locationDto)
    {
        // VERIFY IF USER EXISTS
        var user = await _context.Users.FindAsync(UserId);
        if (user == null)
        {
            throw new Exception("User not found");
        }

        // CREATE LOCATION OBJECT
        var location = new Location
        {
            Name = locationDto.Name,
            Description = locationDto.Description,
            Latitude = locationDto.Latitude,
            Longitude = locationDto.Longitude,
            Address = locationDto.Address,
            UserId = UserId,
            User = user,
            IsActive = true,
            IsVerified = false,
        };

        _context.Locations.Add(location);
        await _context.SaveChangesAsync();

        return new LocationResponseDto
        {
            Id = location.Id,
            Name = location.Name,
            Description = location.Description,
            Latitude = location.Latitude,
            Longitude = location.Longitude,
            Address = location.Address,
            UserId = location.UserId,
            CreatedAt = location.CreatedAt,
            UserFullName = user.First_name + " " + user.Last_name,
        };
    }

    // GET ALL LOCATIONS CREATED BY USERS
    public async Task<List<LocationResponseDto>> GetLocations(
        LocationFilterType filter)
    {
        var query = _context.Locations
            .Include(x => x.User)
            .AsQueryable();

        switch (filter.UserRole.ToLower())
        {
            case "Admin":
                if (filter.UserId.HasValue)
                {
                    query = query.Where(x => x.UserId == filter.UserId.Value);
                }
                break;

            case "User":
                query = query.Where(x =>
                    (filter.UserId.HasValue && x.UserId == filter.UserId.Value) || x.IsVerified);
                break;

            default:
                query = query.Where(x =>
                    x.IsVerified && x.IsActive);
                break;
        }

        // Additional specific filtering
        if (filter.IsVerified.HasValue)
        {
            query = query.Where(l => l.IsVerified == filter.IsVerified.Value);
        }

        if (filter.IsActive.HasValue)
        {
            query = query.Where(l => l.IsActive == filter.IsActive.Value);
        }

        return await query
            .Select(l => new LocationResponseDto
            {
                Id = l.Id,
                Name = l.Name,
                Description = l.Description,
                Latitude = l.Latitude,
                Longitude = l.Longitude,
                Address = l.Address,
                IsActive = l.IsActive,
                IsVerified = l.IsVerified,
                UserId = l.UserId,
                UserFullName = l.User.First_name + " " + l.User.Last_name,
                CreatedAt = l.CreatedAt
            })
            .ToListAsync();
    }

    // GET ALL LOCATIONS CREATED BY A USER
    public async Task<List<LocationResponseDto>> GetUserLocations(Guid userId)
    {
        return await GetLocations(new LocationFilterType
        {
            UserId = userId,
            UserRole = "User"
        });
    }

    // GET ALL LOCATIONS BY ADMIN
    public async Task<List<LocationResponseDto>> GetLocationByAdmin(Guid adminId, bool? isVerified = null)
    {
        // CHECK IF USER IS ADMIN
        var admin = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == adminId && u.Role.ToLower() == "Admin");

        if (admin == null)
        {
            throw new Exception("Admin access required");
        }

        return await GetLocations(new LocationFilterType
        {
            UserId = adminId,
            UserRole = "Admin",
            IsVerified = isVerified
        });
    }

    // ADMIN VERIFY A LOCATION
    public async Task<LocationResponseDto> VerifyLocation(Guid adminId, Guid locationId)
    {
        var admin = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == adminId && u.Role.ToLower() == "Admin");

        if (admin == null)
        {
            throw new Exception("Unauthorized: Admin access required");
        }

        var location = await _context.Locations
            .FirstOrDefaultAsync(l => l.Id == locationId);

        if (location == null)
        {
            throw new Exception("Location not found");
        }

        location.IsVerified = true;
        await _context.SaveChangesAsync();

        return new LocationResponseDto
        {
            Id = location.Id,
            Name = location.Name,
            Description = location.Description,
            Latitude = location.Latitude,
            Longitude = location.Longitude,
            Address = location.Address,
            IsActive = location.IsActive,
            IsVerified = true,
            UserId = location.UserId,
            UserFullName = location.User.First_name + " " + location.User.Last_name,
            CreatedAt = location.CreatedAt
        };
    }

    // TO DELETE A LOCATION BY ADMIN
    public async Task DeleteLocation(Guid adminId, Guid locationId)
    {
        var admin = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == adminId && u.Role.ToLower() == "Admin");

        if (admin == null)
        {
            throw new Exception("Unauthorized: Admin access required");
        }

        var location = await _context.Locations
            .FirstOrDefaultAsync(l => l.Id == locationId);

        if (location == null)
        {
            throw new Exception("Location not found");
        }

        _context.Locations.Remove(location);
        await _context.SaveChangesAsync();
    }

    // TO UPDATE A LOCATION BY USER
    public async Task<LocationResponseDto> UpdateLocation(Guid userId, Guid locationId, LocationCreateDto locationDto)
    {
        var location = await _context.Locations
            .Where(l => l.Id == locationId && l.UserId == userId && !l.IsVerified)
            .FirstOrDefaultAsync();

        if (location == null)
        {
            throw new Exception("Location not found, unauthorized");
        }

        location.Name = locationDto.Name;
        location.Description = locationDto.Description;
        location.Address = locationDto.Address;
        location.Latitude = locationDto.Latitude;
        location.Longitude = locationDto.Longitude;
        location.IsVerified = false;

        await _context.SaveChangesAsync();

        return new LocationResponseDto
        {
            Id = location.Id,
            Name = location.Name,
            Description = location.Description,
            Latitude = location.Latitude,
            Longitude = location.Longitude,
            Address = location.Address,
            IsActive = location.IsActive,
            IsVerified = false,
            UserId = location.UserId,
            UserFullName = location.User.First_name + " " + location.User.Last_name,
            CreatedAt = location.CreatedAt
        };
    }

}