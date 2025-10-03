using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using WSFBackendApi.Data;
using WSFBackendApi.DTOs;
using WSFBackendApi.Models;

namespace WSFBackendApi.Services;

public class AuthService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(ApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<AuthResponseDto> Register(RegisterDto registerDto)
    {
        // Console.WriteLine($"Received Login request for {registerDto.Email}");
        // VALIDATE EMAIL AND PASSWORD INPUT
        if (string.IsNullOrWhiteSpace(registerDto.Email) || string.IsNullOrWhiteSpace(registerDto.Password))
        {
            throw new Exception("Email and password are required");
        }

        // CHECK IF EMAIL ALREADY EXISTS
        if (await _context.Users.AnyAsync(x => x.Email == registerDto.Email))
        {
            throw new UnauthorizedAccessException("Email already exists");
        }

        // HASH PASSWORD BEFORE STORING IN DATABASE
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password);

        // CREATE USER OBJECT BEFOR SENDING TO DATABASE
        var user = new User
        {
            First_name = registerDto.First_name,
            Last_name = registerDto.Last_name,
            Email = registerDto.Email,
            Password = passwordHash,
            Role = "User",
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // GENERATE JWT TOKEN
        var token = GenerateJwtToken(user);

        return new AuthResponseDto
        {
            Token = token,
            UserId = user.Id,
            // RefreshToken = user.RefreshToken
            First_name = user.First_name,
            Last_name = user.Last_name,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            AvatarUrl = user.AvatarUrl,
            CreatedAt = user.CreatedAt,
            LastLogin = user.LastLogin,
            Role = user.Role
        };
    }

    public async Task<AuthResponseDto> Login(LoginDto loginDto)
    {
        Console.WriteLine($"Received Login request for {loginDto.Email}");
        var user = await _context.Users.FirstOrDefaultAsync(x => x.Email.ToLower() == loginDto.Email.ToLower());

        if (user == null)
        {
            Console.WriteLine("User not found");
            throw new Exception("Invalid email or password");
        }
        // VERIFY PASSWORD
        if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, user.Password))
        {
            throw new UnauthorizedAccessException("Invalid password");
        }

        // UPDATE LAST LOGIN TIME
        user.LastLogin = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // GENERATE JWT TOKEN
        var token = GenerateJwtToken(user);

        Console.WriteLine("Login successful");

        return new AuthResponseDto
        {
            Token = token,
            UserId = user.Id,
            // RefreshToken = user.RefreshToken
            First_name = user.First_name,
            Last_name = user.Last_name,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            AvatarUrl = user.AvatarUrl,
            CreatedAt = user.CreatedAt,
            LastLogin = user.LastLogin,
            Role = user.Role
        };
    }

    private bool VerifyPassword(string password, object passwordHash)
    {
        throw new NotImplementedException();
    }

    private string GenerateJwtToken(User user)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Name, $"{user.First_name} {user.Last_name}"),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var jwtKey = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is missing");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(3), // EXPRES IN 3 DAYS
            signingCredentials: creds
        );
        Console.WriteLine($"The token: {token}");
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}