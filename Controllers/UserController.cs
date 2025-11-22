using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WSFBackendApi.DTOs;
using WSFBackendApi.Models;
using WSFBackendApi.Services;

namespace WSFBackendApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController(UserService userService) : ControllerBase
{
    private readonly UserService _userService = userService;

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

            if (string.IsNullOrEmpty(currentUserId))
                return Unauthorized("Invalid user session.");

            // REGULAR USERS CAN ONLY UPDATE THEIR OWN PROFILE
            bool isAdmin = currentUserRole is "Admin" or "super_admin" or "state_admin" or "zonal_admin";
            if (!isAdmin && currentUserId != userId.ToString())
            {
                return Forbid("You cannot update another user details.");
            }

            var updatedProfile = await _userService.UpdateUserProfile(userId, updateDto, currentUserRole);

            return Ok(new
                {
                    message = "Profile updated successfully",
                    data = updatedProfile
                });
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

    [HttpGet("profile/{userId}")]
    [Authorize]
    public async Task<ActionResult<UserProfileResponseDto>> GetUserProfile(Guid userId)
    {
        try
        {
            var response = await _userService.GetUserProfile(userId);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
    {
        try
        {
            // VERIFY USER CHANGING PASSWORD
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Forbid("CALL THE POLICE NOW!!! You cannot change password for another user.");
            }

            var userId = Guid.Parse(currentUserId);

            await _userService.ChangePassword(userId, changePasswordDto);
            return Ok(new { message = "Password changed successfully" });
        }
        catch (Exception ex)
        {
        //    Console.WriteLine($"Registration error: {ex.Message}");
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("all")]
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

    [HttpGet("my-homecell")]
    [Authorize]
    public async Task<IActionResult> GetMyHomeCell()
    {
        try
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(currentUserId) )
            {
                return StatusCode(403, new { message = "You cannot update a location for another user." });
            }

            var userId = Guid.Parse(currentUserId);

            var response = await _userService.GetUserHomeCell(userId);

            return Ok(new { data = response });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

}