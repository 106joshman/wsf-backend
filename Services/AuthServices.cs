using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Http.HttpResults;
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
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password, workFactor: 10);

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

    public async Task<AuthResponseDto> GoogleRegister(GoogleRegisterDto googleRegisterDto)
    {
        // VALIDATE GOOGLE ID TOKEN
        GoogleJsonWebSignature.Payload payload;
        try
        {
            payload = await GoogleJsonWebSignature.ValidateAsync(googleRegisterDto.IdToken);
        }
        catch
        {
            throw new UnauthorizedAccessException("Invalid Google ID Token.");
        }

        if (payload.Email != googleRegisterDto.Email)
            throw new UnauthorizedAccessException("Email does not match request email.");

        // CHECK IF EMAIL ALREADY EXIST ON TABLE
        var existingEmail = await _context.Users.FirstOrDefaultAsync(u => u.Email == googleRegisterDto.Email);
        if (existingEmail != null)
            throw new BadHttpRequestException("User already exists. Please use Google Login.");

        var user = new User
        {
            First_name = googleRegisterDto.First_name,
            Last_name = googleRegisterDto.Last_name,
            Email = googleRegisterDto.Email,
            AvatarUrl = googleRegisterDto.AvatarUrl,
            Password = null,
            Role = "User",
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var token = GenerateJwtToken(user);

        return new AuthResponseDto
        {
            Token = token,
            UserId = user.Id,
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
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Email.Equals(loginDto.Email, StringComparison.CurrentCultureIgnoreCase));

         // CHECK FOR VALID EMAIL AND VERIFY PASSWORD
        if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.Password))
        {
            // Console.WriteLine("User not found");
            throw new Exception("Invalid email or password");
        }

        _ = Task.Run(async () =>
        {
            // UPDATE LAST LOGIN TIME
            user.LastLogin = DateTime.UtcNow;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        });

        // GENERATE JWT TOKEN
        var token = GenerateJwtToken(user);

        return new AuthResponseDto
        {
            Token = token,
            UserId = user.Id,
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

    public async Task<AuthResponseDto> GoogleLogin(GoogleLoginDto googleLoginDto)
    {
        // VALIDATE GOOGLE ID TOKEN
        GoogleJsonWebSignature.Payload payload;
        try
        {
            payload = await GoogleJsonWebSignature.ValidateAsync(googleLoginDto.IdToken);
        }
        catch
        {
            throw new UnauthorizedAccessException("Invalid Google ID Token.");
        }

        // 2️⃣ Compare email with token
        if (payload.Email != googleLoginDto.Email)
           throw new UnauthorizedAccessException("Email in token does not match.");

        // 3️⃣ Find user by GoogleId or Email
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.GoogleId == googleLoginDto.GoogleId || u.Email == googleLoginDto.Email) ?? throw new UnauthorizedAccessException("User not found. Please register first.");

        if (user.GoogleId == null)
        {
            user.GoogleId = googleLoginDto.GoogleId;
            _context.Users.Update(user);
        }

        _ = Task.Run(async () =>
        {
            // UPDATE LAST LOGIN TIME
            user.LastLogin = DateTime.UtcNow;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        });

        // GENERATE JWT TOKEN
        var token = GenerateJwtToken(user);

        return new AuthResponseDto
        {
            Token = token,
            UserId = user.Id,
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
    //    Console.WriteLine($"The token: {token}");
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}