using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using WSFBackendApi.DTOs;
using WSFBackendApi.Services;

namespace WSFBackendApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationController(NotificationService pushService) : ControllerBase
{
    private readonly NotificationService _pushService = pushService;

    [HttpPost("register-token")]
    public async Task<IActionResult> RegisterToken([FromBody] RegisterTokenDto registerTokenDto)
    {
        if (string.IsNullOrWhiteSpace(registerTokenDto.Token))
            return BadRequest(new { message = "Token is required. "});

        try
        {
            await _pushService.RegisterToken(registerTokenDto);
            return Ok(new { success = true });
        }
        catch  (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
