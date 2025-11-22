using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WSFBackendApi.DTOs;
using WSFBackendApi.Services;

namespace WSFBackendApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OutlineController(OutlineService outlineService) : ControllerBase
{
    private readonly OutlineService _outlineService = outlineService;

    // Fixed existing endpoint
    [HttpPost("teaching/create")]
    [Authorize(Roles = "super_admin,Admin,state_admin,zonal_admin")]
    public async Task<IActionResult> CreateTeaching([FromBody] TeachingCreateDto dto)
    {
        var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var createdBy = User.FindFirstValue(JwtRegisteredClaimNames.Name)
            ?? User.FindFirstValue("name")
            ?? User.FindFirstValue(ClaimTypes.Email)
            ?? "Admin";

        if (string.IsNullOrEmpty(adminId))
            return Unauthorized("User not authenticated");

        var response = await _outlineService.CreateTeachingAsync(dto, Guid.Parse(adminId), createdBy ?? "");
        return Ok(response);
    }

    // New endpoint for creating prayer outline
    [HttpPost("prayer/create")]
    [Authorize(Roles = "super_admin,Admin,state_admin,zonal_admin")]
    public async Task<IActionResult> CreatePrayerOutline([FromBody] PrayerOutlineCreateDto dto)
    {
        var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var createdBy = User.FindFirstValue(JwtRegisteredClaimNames.Name)
            ?? User.FindFirstValue("name")
            ?? User.FindFirstValue(ClaimTypes.Email)
            ?? "Admin";

        if (string.IsNullOrEmpty(adminId))
            return Unauthorized("User not authenticated");

        var response = await _outlineService.CreatePrayerOutlineAsync(dto, Guid.Parse(adminId), createdBy ?? "");
        return Ok(response);
    }

    // New endpoint for creating complete monthly schedule
    [HttpPost("monthly-schedule/create")]
    [Authorize(Roles = "super_admin,Admin,state_admin,zonal_admin")]
    public async Task<IActionResult> CreateMonthlySchedule([FromBody] MonthlyScheduleCreateDto dto)
    {
        var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var createdBy = User.FindFirstValue(JwtRegisteredClaimNames.Name)
            ?? User.FindFirstValue("name")
            ?? User.FindFirstValue(ClaimTypes.Email)
            ?? "Admin";

        if (string.IsNullOrEmpty(adminId))
            return Unauthorized("User not authenticated");

        try
        {
            var response = await _outlineService.CreateMonthlyScheduleAsync(dto, Guid.Parse(adminId), createdBy ?? "");
            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest($"Failed to create monthly schedule: {ex.Message}");
        }
    }

    // Get monthly schedule by month
    [HttpGet("monthly-schedule")]
    // [Authorize(Roles = "super_admin,Admin,state_admin,zonal_admin")]
    public async Task<IActionResult> GetMonthlySchedules([FromQuery] string? month, [FromQuery] int? year)
    {
        var targetYear = year ?? DateTime.UtcNow.Year;

        if (string.IsNullOrEmpty(month))
        {
            // Return all if no month/year provided
            var allSchedules = await _outlineService.GetSchedulesByYearAsync(targetYear);
            return Ok(new { data = allSchedules });
        }

        var response = await _outlineService.GetMonthlyScheduleAsync(month, year ?? DateTime.UtcNow.Year);

        if (response == null)
            return NotFound($"No schedule found for {month} {year}");

        return Ok(new { data = response });
    }

    // Get all monthly schedules
    // [HttpGet("monthly-schedule")]
    // [Authorize(Roles = "super_admin,Admin,state_admin,zonal_admin")]
    // public async Task<IActionResult> GetAllMonthlySchedules()
    // {
    //     try
    //     {
    //         var response = await _outlineService.GetAllMonthlySchedulesAsync();
    //         return Ok(new {data = response});
    //     }
    //     catch (Exception ex)
    //     {
    //     //    Console.WriteLine($"Error: {ex.Message}");
    //         return StatusCode(500, new { message = "An error occurred while fetching schedules." });
    //     }
    // }
}
