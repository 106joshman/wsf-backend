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
            Contact = locationDto.Contact,
            District = locationDto.District,
            State =locationDto.State,
            Country =locationDto.Country,
            LGA =locationDto.LGA,
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
            Contact = locationDto.Contact,
            District = locationDto.District,
            State =locationDto.State,
            Country =locationDto.Country,
            LGA =locationDto.LGA,
            UserId = location.UserId,
            CreatedAt = location.CreatedAt,
            UserFullName = user.First_name + " " + user.Last_name,
        };
    }

    // GET ALL VERIFIED LOCATIONS CREATED BY USERS
    public async Task<PaginatedResponse<LocationResponseDto>> GetVerifiedLocations(PaginationParams paginationParams)
    {
        var query = _context.Locations
            .Include(x => x.User)
            .Where(x => x.IsVerified && x.IsActive);

        var totalCount = await query.CountAsync();

        var locationList = await query
            .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
            .Take(paginationParams.PageSize)
            .Select(l => new LocationResponseDto
            {
                Id = l.Id,
                Name = l.Name,
                Description = l.Description,
                Latitude = l.Latitude,
                Longitude = l.Longitude,
                Address = l.Address,
                Contact = l.Contact,
                District = l.District,
                State =l.State,
                Country =l.Country,
                LGA =l.LGA,
                IsActive = l.IsActive,
                IsVerified = l.IsVerified,
                UserId = l.UserId,
                UserFullName = l.User.First_name + " " + l.User.Last_name,
                CreatedAt = l.CreatedAt
            })
            .ToListAsync();

        var paginatedResult = new PaginatedResponse<LocationResponseDto>
        {
            Items = locationList,
            PageNumber = paginationParams.PageNumber,
            PageSize = paginationParams.PageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)paginationParams.PageSize)
        };

        return paginatedResult;
    }

    // GET ALL LOCATIONS CREATED BY A USER
    public async Task<List<LocationResponseDto>> GetUserLocations(Guid UserId)
    {
        var query = _context.Locations
            .Include(x => x.User)
            .Where(x => x.UserId == UserId);

        return await query
            .Select(l => new LocationResponseDto
            {
                Id = l.Id,
                Name = l.Name,
                Description = l.Description,
                Latitude = l.Latitude,
                Longitude = l.Longitude,
                Address = l.Address,
                Contact = l.Contact,
                District = l.District,
                State =l.State,
                Country =l.Country,
                LGA =l.LGA,
                IsActive = l.IsActive,
                IsVerified = l.IsVerified,
                UserId = l.UserId,
                UserFullName = l.User.First_name + " " + l.User.Last_name,
                CreatedAt = l.CreatedAt
            })
            .ToListAsync();
    }

    // GENERAL LOCATION FILTERING FOR ADMINS
    public async Task<PaginatedResponse<LocationResponseDto>> GetLocations(LocationFilterType filter, PaginationParams paginationParams)
    {
        var query = _context.Locations
            .Include(x => x.User)
            .AsQueryable();

        // ROLE BASED FILTERING
        var allowedRoles = new[] { "admin", "super_admin" };

        // Admin-specific filtering
        if (filter.UserRole?.ToLower() == "super_admin" || filter.UserRole?.ToLower() == "admin")
        {
            if (filter.UserId.HasValue)
            {
                query = query.Where(x => x.UserId == filter.UserId.Value);
            }
            // Otherwise, admin can see all locations
        }
        else
        {
            // Default filter for non-admin cases
            query = query.Where(x => x.IsVerified && x.IsActive);
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

        var totalCount = await query.CountAsync();

        var locationList = await query
            .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
            .Take(paginationParams.PageSize)
            .Select(l => new LocationResponseDto
            {
                Id = l.Id,
                Name = l.Name,
                Description = l.Description,
                Latitude = l.Latitude,
                Longitude = l.Longitude,
                Address = l.Address,
                Contact = l.Contact,
                District = l.District,
                State = l.State,
                Country = l.Country,
                LGA = l.LGA,
                IsActive = l.IsActive,
                IsVerified = l.IsVerified,
                UserId = l.UserId,
                UserFullName = l.User.First_name + " " + l.User.Last_name,
                CreatedAt = l.CreatedAt
            })
            .ToListAsync();

        return new PaginatedResponse<LocationResponseDto>
        {
            Items = locationList,
            PageNumber = paginationParams.PageNumber,
            PageSize = paginationParams.PageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)paginationParams.PageSize)
        };

    }

    // GET ALL LOCATIONS BY ADMIN
    public async Task<PaginatedResponse<LocationResponseDto>> GetLocationByAdmin(Guid adminId, bool? isVerified = null)
    {
        // CHECK IF USER IS ADMIN
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == adminId && (u.Role.ToLower() == "Admin" || u.Role.ToLower() == "super_admin"));

        if (user == null)
        {
            throw new Exception("Access denied: Admin access required");
        }

        return await GetLocations(new LocationFilterType
        {
            UserId = adminId,
            UserRole = user.Role,
            IsVerified = isVerified
        }, new PaginationParams());
    }

    // ADMIN VERIFY A LOCATION
    public async Task<LocationResponseDto> VerifyLocation(Guid adminId, Guid locationId)
    {
        var admin = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == adminId && (u.Role.ToLower() == "Admin" || u.Role.ToLower() == "super_admin"));

        if (admin == null)
        {
            throw new Exception("Unauthorized: Admin access required");
        }

        var location = await _context.Locations
            .Include(l => l.User)
            .FirstOrDefaultAsync(l => l.Id == locationId);

        if (location == null)
        {
            throw new Exception("Location not found");
        }

        if (location.IsVerified)
        {
            throw new Exception("Location is already verified.");
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
            Contact = location.Contact,
            District = location.District,
            State =location.State,
            Country =location.Country,
            LGA =location.LGA,
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
            .FirstOrDefaultAsync(u => u.Id == adminId && (u.Role.ToLower() == "Admin" || u.Role.ToLower() == "super_admin"));

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
    public async Task<LocationResponseDto> UpdateLocation(Guid UserId, Guid locationId, UpdateLocationDto updateDto)
    {
        var location = await _context.Locations
            .Include(l => l.User)
            .Where(l => l.Id == locationId && l.UserId == UserId && !l.IsVerified)
            .FirstOrDefaultAsync();

        if (location is null)
        {
            throw new Exception("Location not found, unauthorized");
        }

        if (!string.IsNullOrWhiteSpace(updateDto.Name))
            location.Name = updateDto.Name;
        if (!string.IsNullOrWhiteSpace(updateDto.Description))
            location.Description = updateDto.Description;
        if (!string.IsNullOrWhiteSpace(updateDto.Contact))
            location.Contact = updateDto.Contact;
        if (!string.IsNullOrWhiteSpace(updateDto.District))
            location.District = updateDto.District;
        if (!string.IsNullOrWhiteSpace(updateDto.Address))
            location.Address = updateDto.Address;

        // RUN LOCATION UPDATE BY USER
        _context.Locations.Update(location);
        // UPDATE CHANGES TO THE DATABASE
        await _context.SaveChangesAsync();

        return new LocationResponseDto
        {
            Id = location.Id,
            Name = location.Name,
            Description = location.Description,
            Latitude = location.Latitude,
            Longitude = location.Longitude,
            Address = location.Address,
            Contact = location.Contact,
            District = location.District,
            State =location.State,
            Country =location.Country,
            LGA =location.LGA,
            IsActive = location.IsActive,
            IsVerified = false,
            UserId = location.UserId,
            UserFullName = location.User.First_name + " " + location.User.Last_name,
            CreatedAt = location.CreatedAt
        };
    }

}