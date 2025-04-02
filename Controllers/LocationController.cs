using Microsoft.AspNetCore.Mvc;
using WSFBackendApi.Data;
using WSFBackendApi.DTOs;
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

    // CREATE LOCATION
    [HttpPost]
    public async Task<IActionResult> CreateLocation(Guid UserId, [FromBody] LocationCreateDto locationDto)
    {
        try
        {
            var response = await _locationService.CreateLocation(UserId, locationDto);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // GET LOCATION BASED ON FILTER OF AVAILABLE USER OR NOT
    [HttpGet]
    public async Task<IActionResult> GetLocations([FromQuery] LocationFilterType filter)
    {
        try
        {
            var response = await _locationService.GetLocations(filter);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // GET LOCATIOPN BY ID
    [HttpGet("{locationId}")]
    public async Task<IActionResult> GetLocationById(Guid locationId)
    {
        try
        {
            var filter = new LocationFilterType
            {
                UserId = null,
                UserRole = "Admin"
            };

            // TO GET LOCATION WITH SPECIFIC ID
            var locations = await _locationService.GetLocations(filter);

            // FIND THE SPECIFIC LOCATION
            var location = locations.FirstOrDefault(l => l.Id == locationId);

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

    // UPDATE LOCATION BY CREATOR ONLY IF LOCATION IS UNVERIFIED BY ADMIN
    [HttpPut("{locationId}")]
    public async Task<IActionResult> UpdateLocation(Guid userId, Guid locationId, [FromBody] LocationCreateDto locationDto)
    {
        try
        {
            var updatedLocation = await _locationService.UpdateLocation(userId, locationId, locationDto);
            return Ok(updatedLocation);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // DELETE LOCATION ONLY BY ADMIN
    [HttpDelete("{locationId}")]
    public async Task<IActionResult> DeleteLocation(Guid adminId, Guid locationId)
    {
        try
        {
            // VERIFY ADMIN AND DELETE LOCATION
            await _locationService.DeleteLocation(adminId, locationId);
            return Ok("Location deleted successfully");
        }
        catch (Exception ex)
        {

            return BadRequest(ex.Message);
        }
    }
}