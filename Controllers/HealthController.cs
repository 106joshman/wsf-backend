using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WSFBackendApi.Data;

namespace WSFBackendApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public HealthController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetHealth()
    {
        var health = new
        {
            ststus = "API is running",
            database = "Connected!!!",
            time = DateTime.UtcNow
        };

        try
        {
            await _context.Database.ExecuteSqlRawAsync("SELECT 1");
            return Ok(health);
        }
        catch (System.Exception)
        {

            var degraded = new
            {
                status = "API is running",
                database = "Disconnected!!!",
                error = "Database connection failed",
                time = DateTime.UtcNow
            };

            return StatusCode(503, degraded);
        }
    }
}