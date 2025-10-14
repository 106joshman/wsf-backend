using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WSFBackendApi.DTOs;
using WSFBackendApi.Models;
using WSFBackendApi.Services;

namespace WSFBackendApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly UserService _userService;
    public UserController(UserService userService)
    {
        _userService = userService;
    }

    // UPDATE USER PROFLE
    [HttpPut("profile/{userId}")]
    [Authorize]
    public async Task<ActionResult<UserProfileResponseDto>> UpdateProfile(Guid userId, [FromBody] UserUpdateDto updateDto)
    {
        try
        {
            // VERIFY USER BEFORE PROFILE UPDATE
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentUserRole = User.FindFirstValue(ClaimTypes.Role) ?? "User";
            // Console.WriteLine($"Token user ID: {currentUserId}, URL user ID: {userId}");
            if (string.IsNullOrEmpty(currentUserId) || (currentUserId != userId.ToString()))
            {
                return Forbid("You cannot update another user details.");
            }

            var updatedProfile = await _userService.UpdateUserProfile(userId, updateDto, currentUserRole);

            return Ok(updatedProfile);
        }
        catch (UnauthorizedAccessException ex)
        {
            // 401 ERROR
        //    Console.WriteLine($"USER UPDATE ERROR: {ex.Message}"); // Debugging log
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

    [HttpGet("profile/{userId}")]
    [Authorize]
    public async Task<ActionResult<UserProfileResponseDto>> GetUserProfile(Guid userId)
    {
        try
        {
            // VERIFY USER BEFORE GETTING PROFILE
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (currentUserId != userId.ToString())
            {
                return Forbid("FRAUD!!! You cannot profile for another user.");
            }

            var profile = await _userService.GetUserProfile(userId);
            return Ok(profile);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    [HttpPost("change-password/{userId}")]
    [Authorize]
    public async Task<IActionResult> ChangePassword(Guid userId, [FromBody] ChangePasswordDto changePasswordDto)
    {
        try
        {
            // VERIFY USER CHANGING PASSWORD
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (currentUserId != userId.ToString())
            {
                return Forbid("CALL THE POLICE NOW!!! You cannot change password for another user.");
            }

            await _userService.ChangePassword(userId, changePasswordDto);
            return Ok(new { message = "Password changed successfully" });
        }
        catch (Exception ex)
        {
        //    Console.WriteLine($"Registration error: {ex.Message}");
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("all")]
    [Authorize]
    public async Task<IActionResult> GetAllUsers(
        [FromQuery] PaginationParams pagination,
        [FromQuery] string? First_name,
        [FromQuery] string? Last_name,
        [FromQuery] string? email
    )
    {
        try
        {
            var response = await _userService.GetAllUsers(pagination, First_name, Last_name, email);
            return Ok(response);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching users: {ex.Message}");
            return StatusCode(500, new { message = "An error occurred while fetching users." });
        }
    }
}