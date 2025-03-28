using Microsoft.AspNetCore.Mvc;
using WSFBackendApi.DTOs;
using WSFBackendApi.Services;

namespace WSFBackendApi.Controllers;
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;
    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
    {
        // Console.WriteLine("Received Register request"); // Debugging log
        try
        {
            var response = await _authService.Register(registerDto);
            // Console.WriteLine("Registration successful"); // Debugging log
            return Ok(response);
        }
        catch (Exception ex)
        {
            // Console.WriteLine($"Registration error: {ex.Message}"); // Debugging log
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        // Console.WriteLine($"Received Login request: {loginDto.Email}"); // Debugging log
        try
        {
            var response = await _authService.Login(loginDto);
            // Console.WriteLine($"Login successful for : {loginDto.Email}");
            return Ok(response);
        }
        catch (Exception ex)
        {
            // Console.WriteLine($"Login error: {ex.Message}");
            return BadRequest(ex.Message);
        }
    }
}