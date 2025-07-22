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
    [HttpPost("{userId}")]
    [Authorize]
    public async Task<IActionResult> CreateLocation(Guid userId, [FromBody] LocationCreateDto locationDto)
    {
        try
        {
            // VERIFY USER TOKEN BEFORE LOCATION IS CREATED
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (currentUserId != userId.ToString())
            {
                return Forbid();
            }

            var response = await _locationService.CreateLocation(userId, locationDto);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            // 401 ERROR
            Console.WriteLine($"THIS IS A 401 ERROR: {ex.Message}"); // Debugging log
            return Unauthorized(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            // 404 ERROR
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Registration error: {ex.Message}"); // Debugging log
            return BadRequest(new { message = ex.Message });
        }
    }

    // // GET LOCATION BY ID BY ADMIN/SUPER_ADMIN/USER/GUEST
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

    // GET ALL LOCATIONS BY USER ID
    [HttpGet("user/{userId}")]
    [Authorize]
    public async Task<IActionResult> GetUserLocations(Guid userId)
    {
        try
        {
            // Print all claims for debugging
            foreach (var claim in User.Claims)
            {
                Console.WriteLine($"Claim Type: {claim.Type}, Value: {claim.Value}");
            }

            // VERIFY USER BEFORE YOU GET USER LOCATION
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            // Console.WriteLine($"Token user ID: {currentUserId}, URL user ID: {userId}");
            if (currentUserId != userId.ToString())
            {
                return Forbid();
            }

            // Log the received userId for debugging
        // Console.WriteLine($"Received request for user locations with userId: {userId}");

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
            Console.WriteLine($"Error in GetUserLocations: {ex.Message}");
            return BadRequest(ex.Message);
        }
    }

    // GET ALL LOCATIONS BY ADMIN ROLE
    [HttpGet("admin")]
    [Authorize(Roles = "Admin,super_admin")]
    public async Task<IActionResult> GetLocationsByAdmin([FromQuery] LocationFilterType filter, [FromQuery] PaginationParams pagination)
    {
        try
        {
            var currentUserRole = User.FindFirstValue(ClaimTypes.Role);
            // Validate the admin role before proceeding
            var allowedRoles = new[] { "Admin", "super_admin"};
            if (string.IsNullOrEmpty(currentUserRole) || !allowedRoles.Contains(currentUserRole, StringComparer.OrdinalIgnoreCase))
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
    [HttpPut("verify/{adminId}/{locationId}")]
    [Authorize(Roles = "Admin,super_admin")]
    public async Task<IActionResult> VerifyLocation(Guid adminId, Guid locationId)
    {
        try
        {
            // VERIFY SPECIFIC ADMIN ROLE AND TOKEN BEFOR VERIFICATION
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentUserRole = User.FindFirstValue(ClaimTypes.Role);

            if (currentUserId != adminId.ToString())
            {
                return Forbid("Token user ID does not match admin ID");
            }

            if (!new[] { "Admin", "super_admin" }.Contains(currentUserRole, StringComparer.OrdinalIgnoreCase))
            {
                return Unauthorized("Admin access required for location verification");
            }

            var response = await _locationService.VerifyLocation(adminId, locationId);
            return Ok(new { message = "Location verified successfully", });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Verification error: {ex.Message}");
            return BadRequest(new { message = ex.Message });
        }
    }

    // UPDATE LOCATION BY CREATOR ONLY IF LOCATION IS UNVERIFIED BY ADMIN
    [HttpPut("{userId}/{locationId}")]
    [Authorize]
    public async Task<IActionResult> UpdateLocation(Guid userId, Guid locationId, [FromBody] UpdateLocationDto updateDto)
    {
        try
        {
            // VERIFY USER BEFORE YOU GET USER LOCATION
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Console.WriteLine($"Token user ID: {currentUserId}, URL user ID: {userId}");
            if (currentUserId != userId.ToString())
            {
                return Forbid();
            }

            var updatedLocation = await _locationService.UpdateLocation(userId, locationId, updateDto);
            return Ok(updatedLocation);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // DELETE LOCATION ONLY BY ADMIN
    [HttpDelete("{adminId}/{locationId}")]
    [Authorize(Roles = "Admin,super_admin")]
    public async Task<IActionResult> DeleteLocation(Guid adminId, Guid locationId)
    {
        try
        {
            var tokenAdminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
var tokenRole = User.FindFirstValue(ClaimTypes.Role);
Console.WriteLine($"Token UserId: {tokenAdminId}, Role: {tokenRole}");

            var currentUserRole = User.FindFirstValue(ClaimTypes.Role);
                Console.WriteLine($"Current user role: {currentUserRole}");

            // VERIFY ADMIN AND DELETE LOCATION
            await _locationService.DeleteLocation(adminId, locationId);
            return Ok("Location deleted successfully");
        }
        catch (Exception ex)
        {
             Console.WriteLine($"CHECK THIS ERROR: {ex.Message}");
            return BadRequest(new { message = ex.Message });
        }
    }
}