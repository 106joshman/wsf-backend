using System.Security.Claims;
using BCrypt.Net;
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
public class AdminController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserService _userService;
    private readonly AdminAuthService _adminAuthService;
    public AdminController(ApplicationDbContext context, UserService userService, AdminAuthService adminAuthService)
    {
        _context = context;
        _userService = userService;
        _adminAuthService = adminAuthService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> AdminLogin([FromBody] LoginDto loginDto)
    {
        try
        {
            var response = await _adminAuthService.AdminLogin(loginDto);
            Console.WriteLine($"Login successful for : {response}");

            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            // 401 ERROR
            return Unauthorized(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            // 404 ERROR
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            // Console.WriteLine($"Registration error: {ex.Message}"); // Debugging log
            return BadRequest(new { message = ex.Message });
        }
    }

    // GET SINGLE ADMIN DETAILS
    [HttpGet("{id}")]
    [Authorize(Roles = "super_admin, Admin, state_admin, zonal_admin")]
    public async Task<ActionResult> GetAdmin(Guid id)
    {
        try
        {
            var response = await _adminAuthService.GetAdminById(id);
            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // CREATE AN ADMIN ACCOUNT
    [HttpPost("register-admin")]
    [Authorize(Roles = "super_admin")]
    public async Task<IActionResult> RegisterAdmin([FromBody] AdminRegisterDto adminRegisterDto)
    {
        try
        {
            var superAdminIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(superAdminIdClaim) || !Guid.TryParse(superAdminIdClaim, out Guid superAdminId))
            {
                return Unauthorized(new { message = "Invalid or missing admin token." });
            }
            var response = await _adminAuthService.CreateAdmin(superAdminId, adminRegisterDto);

            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            // 401 ERROR
            return Unauthorized(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            // 404 ERROR
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Admin Registration error: {ex.Message}"); // Debugging log
            return BadRequest(new { message = ex.Message });
        }
    }


    // DELETE AN ADMIN
    [HttpDelete("{id?}")]
    [Authorize(Roles = "super_admin")]
    public async Task<IActionResult> DeleteAdmin(Guid? id, [FromBody] DeleteAdminsRequest request)
    {
        var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(currentUserId) || !Guid.TryParse(currentUserId, out Guid superAdminId))
        {
            return Unauthorized(new { message = "Invalid user token." });
        }

        // VERIFY LOGGED IN USER TO BE SUPER ADMIN
        var superAdmin = await _context.Admins.FirstOrDefaultAsync(x => x.Id == superAdminId && x.Role.ToLower() == "super_admin");

        if (superAdmin == null)
        {
            return Unauthorized(new { message = "Only super admin can delete an admin" });
        }

        var idsToDelete = new List<Guid>();

        if (id.HasValue)
        {
            idsToDelete.Add(id.Value);
        }
        else if (request.Ids != null && request.Ids.Any())
        {
            idsToDelete.AddRange(request.Ids);
        }
        else
        {
            return BadRequest(new { message = "No admin provided to be deleted." });
        }

        // Prevent self-deletion
        if (request.Ids.Contains(superAdminId))
        {
            return BadRequest(new { message = "You cannot delete your own account" });
        }

        // Find admin to delete
        var adminsToDelete = await _context.Admins
            .Where(x => request.Ids.Contains(x.Id))
            .ToListAsync();

        if (!adminsToDelete.Any())
        {
            return NotFound(new { message = "Admin(s) not found" });
        }

        var deletedIds = adminsToDelete.Select(x => x.Id).ToList();
        var notFoundIds = idsToDelete.Except(deletedIds).ToList();

        _context.Admins.RemoveRange(adminsToDelete);
        await _context.SaveChangesAsync();

        return Ok(new {
            message = $"Successfully deleted {deletedIds.Count} admin(s)",
            deletedIds,
            notFoundIds,
            totalRequested = deletedIds.Count,
            totalDeleted = deletedIds.Count
        });
    }


    [HttpGet("all")]
    public async Task<IActionResult> GetAllAdmin(
        [FromQuery] PaginationParams pagination,
        [FromQuery] string? First_name,
        [FromQuery] string? Last_name,
        [FromQuery] string? email,
        [FromQuery] string? state
    )
    {
        var result = await _userService.GetAllAdmins(pagination, First_name, Last_name, email, state);
        return Ok(result);
    }
}
