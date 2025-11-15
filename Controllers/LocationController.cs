using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WSFBackendApi.Data;
using WSFBackendApi.DTOs;
using WSFBackendApi.Models;
using WSFBackendApi.Services;

namespace WSFBackendApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LocationController : ControllerBase
{
    private readonly LocationService _locationService;
    private readonly ApplicationDbContext _context;

    public LocationController(LocationService locationService, ApplicationDbContext context)
    {
        _locationService = locationService;
        _context = context;
    }

    // CREATE LOCATION ENDPOINT
    [HttpPost("create")]
    [Authorize]
    public async Task<IActionResult> CreateLocation([FromBody] LocationCreateDto locationDto)
    {
        try
        {
            // VERIFY USER TOKEN BEFORE LOCATION IS CREATED
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentUserRole = User.FindFirstValue(ClaimTypes.Role);

            if (string.IsNullOrEmpty(currentUserId) || string.IsNullOrEmpty(currentUserRole))
            {
                return Unauthorized(new { message = "Invalid or missing user credentials" });
            }

            var userId = Guid.Parse(currentUserId);

            // ✅ Allow only specific roles to create locations
            var allowedRoles = new[] {UserRoles.HomeCellLeader};

            if (!allowedRoles.Contains(currentUserRole))
                return StatusCode(403, new { message = "You do not have permission to create a location." });

            // ✅ Prevent users from creating locations for other users
            if (currentUserId != userId.ToString())
                return StatusCode(403, new { message = "You cannot create a location for another user." });

            var response = await _locationService.CreateLocation(userId, locationDto);
            return Ok(new { message = "Location created successfully", data = response });
        }
        catch (UnauthorizedAccessException ex)
        {
            // 401 ERROR
            // Console.WriteLine($"THIS IS A 401 ERROR: {ex.Message}"); // Debugging log
            return Unauthorized(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            // 404 ERROR
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
        //    Console.WriteLine($"Registration error: {ex.Message}"); // Debugging log
            return BadRequest(new { message = ex.Message });
        }
    }

    // GET LOCATION BY ID BY ADMIN/SUPER_ADMIN/USER/GUEST
    [HttpGet("{locationId}")]
    public async Task<IActionResult> GetLocationById(Guid locationId)
    {
        try
        {
            // TO GET LOCATION WITH SPECIFIC ID
            var location = await _context.Locations
                .Include(x => x.User)
                .Where(l => l.Id == locationId)
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
                    UserId = l.UserId,
                    CreatedAt = l.CreatedAt,
                    UserFullName = l.User.First_name + " " + l.User.Last_name,
                })
                .FirstOrDefaultAsync();

            if (location == null)
            {
                return NotFound("Location not found");
            }

            return Ok(location);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // GET ALL LOCATIONS BY USER ID
    [HttpGet("user/{userId}")]
    [Authorize]
    public async Task<IActionResult> GetUserLocations(Guid userId)
    {
        try
        {
            // VERIFY USER BEFORE YOU GET USER LOCATION
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (currentUserId != userId.ToString())
            {
                return StatusCode(403, new { message = "You do not have the clearance for this location!" });
            }

            // Validate the userId format
            if (userId == Guid.Empty)
            {
                return BadRequest("Invalid user ID format");
            }

                var response = await _locationService.GetUserLocations(userId);
                return Ok(response);
            }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // GET ALL LOCATIONS THAT HAS BEEN VERIFIED
    [HttpGet("all")]
    public async Task<IActionResult> GetVerifiedLocations([FromQuery] PaginationParams paginationParams)
    {
        try
        {
            var response = await _locationService.GetVerifiedLocations(paginationParams);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // GET ALL LOCATIONS BY ADMIN ROLE
    [HttpGet("admin")]
    [Authorize(Roles = "super_admin,Admin,state_admin,zonal_admin")]
    public async Task<IActionResult> GetLocationsByAdmin([FromQuery] LocationFilterType filter, [FromQuery] PaginationParams pagination)
    {
        try
        {
            var currentUserRole = User.FindFirstValue(ClaimTypes.Role);
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Validate the admin role before proceeding
            var allowedRoles = new[] { "Admin", "super_admin", "state_admin", "zonal_admin"};
            if (string.IsNullOrEmpty(currentUserId) || !allowedRoles.Contains(currentUserRole, StringComparer.OrdinalIgnoreCase))
            {
                return Unauthorized("Admin access required");
            }
            // SET USER ROLE IN THE FILTER FOR SERVICE PROCESSING
            filter.UserRole = currentUserRole;

            var response = await _locationService.GetLocations(filter,pagination);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // LOCATION VERIFICATION ENPOINT BY ADMIN
    [HttpPut("verify/{locationId}")]
    [Authorize(Roles = "Admin,super_admin,state_admin,zonal_admin")]
    public async Task<IActionResult> VerifyLocation(Guid locationId)
    {
        try
        {
            // VERIFY SPECIFIC ADMIN ROLE AND TOKEN BEFOR VERIFICATION
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentUserRole = User.FindFirstValue(ClaimTypes.Role);

            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized("user ID not found");
            }

            if (!new[] { "Admin", "super_admin", "state_admin", "zonal_admin" }.Contains(currentUserRole, StringComparer.OrdinalIgnoreCase))
            {
                return Unauthorized(new { message = "Admin access required for location verification" });
            }

            if (!Guid.TryParse(currentUserId, out var adminId))
            {
                return BadRequest(new {message = "Invalid user ID"});
            }

            // Optional: Verify the admin exists in database (for extra security)
            var adminExists = await _context.Admins.AnyAsync(u => u.Id == adminId &&
                new[] { "Admin", "super_admin", "state_admin", "zonal_admin" }.Contains(u.Role.ToLower()));

            if (!adminExists)
            {
                return Unauthorized(new {message="Admin user not found or invalid role"});
            }

            var response = await _locationService.VerifyLocation(locationId);
            return Ok(new
            {
                message = "Location verified successfully",
                // location = response
            });
        }
        catch (Exception ex)
        {
        //    Console.WriteLine($"Verification error: {ex.Message}");
            return BadRequest(new { message = ex.Message });
        }
    }

    // UPDATE LOCATION BY CREATOR ONLY IF LOCATION IS UNVERIFIED BY ADMIN
    [HttpPut("{locationId}")]
    [Authorize]
    public async Task<IActionResult> UpdateLocation(Guid locationId, [FromBody] UpdateLocationDto updateDto)
    {
        try
        {
            // VERIFY USER BEFORE YOU GET USER LOCATION
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(currentUserId) )
            {
                return Forbid("You cannot update a location for another user.");
            }

            var userId = Guid.Parse(currentUserId);

            var updatedLocation = await _locationService.UpdateLocation(userId, locationId, updateDto);
            return Ok(updatedLocation);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // DELETE LOCATION ONLY BY ADMIN
    [HttpDelete("{locationId}")]
    [Authorize(Roles = "super_admin,Admin,state_admin,zonal_admin")]
    public async Task<IActionResult> DeleteLocation(Guid locationId)
    {
        try
        {
            var currentAdminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentAdminRole = User.FindFirstValue(ClaimTypes.Role);


            if (string.IsNullOrEmpty(currentAdminId))
            {
                return Unauthorized("user ID not found");
            }

            if (!new[] { "Admin", "super_admin", "state_admin", "zonal_admin" }.Contains(currentAdminRole, StringComparer.OrdinalIgnoreCase))
            {
                return Unauthorized(new { message = "Admin access required for location verification" });
            }

            if (!Guid.TryParse(currentAdminId, out var adminId))
            {
                return BadRequest(new {message = "Invalid user ID"});
            }

            // Optional: Verify the admin exists in database (for extra security)
            var adminExists = await _context.Admins.AnyAsync(u => u.Id == adminId &&
                new[] { "Admin", "super_admin", "state_admin", "zonal_admin" }.Contains(u.Role.ToLower()));

            if (!adminExists)
            {
                return Unauthorized(new {message="Admin user not found or invalid role"});
            }

            // VERIFY ADMIN AND DELETE LOCATION
            await _locationService.DeleteLocation(locationId);
            return Ok(new { message = "Location deleted successfully" });
        }
        catch (Exception ex)
        {
         //    Console.WriteLine($"CHECK THIS ERROR: {ex.Message}");
            return BadRequest(new { message = ex.Message });
        }
    }
}