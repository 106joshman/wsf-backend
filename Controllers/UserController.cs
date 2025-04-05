using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WSFBackendApi.DTOs;
using WSFBackendApi.Services;

namespace WSFBackendApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly UserService _userService;
    public UserController (UserService userService)
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
            // VERIFY USER CHANGING PASSWORD
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (currentUserId != userId.ToString())
            {
                return Forbid();
            }

            var updatedProfile = await _userService.UpdateUserProfile(userId, updateDto);
            return Ok(updatedProfile);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("profile/{userId}")]
    [Authorize]
    public async Task<ActionResult<UserProfileResponseDto>> GetUserProfile(Guid userId)
    {
        try
        {
            // Optional: Verify that the current user is accessing their own profile
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (currentUserId != userId.ToString())
            {
                return Forbid();
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
                return Forbid();
            }

            await _userService.ChangePassword(userId, changePasswordDto);
            return Ok(new {message = "Password changed successfully"});
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Registration error: {ex.Message}");
            return BadRequest(ex.Message);
        }
    }

}