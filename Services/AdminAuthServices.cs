using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using WSFBackendApi.Data;
using WSFBackendApi.DTOs;
using WSFBackendApi.Models;

namespace WSFBackendApi.Services;
public class AdminAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public AdminAuthService(ApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<AdminCreateResponseDto> CreateAdmin(Guid superAdminId, AdminRegisterDto registerDto)
    {
        var superAdmin = await _context.Admins.Where(a => a.Id == superAdminId && a.Role.ToLower() == "super_admin")
        .FirstOrDefaultAsync();

        if (superAdmin == null)
        {
            throw new UnauthorizedAccessException("Only super admin can create admins");
        }

        if (await _context.Admins.AnyAsync(x => x.Email == registerDto.Email))
        {
            throw new Exception("Email already exists");
        }

        var allowedRoles = new[] { "Admin", "state_admin", "zonal_admin" };

        if (!allowedRoles.Contains(registerDto.Role, StringComparer.OrdinalIgnoreCase))
        {
            throw new Exception("Invalid admin role specified.");
        }

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password, workFactor: 10);

        // CREATE ADMIN OBJECT BEFORE SENDING TO DATABASE
        var admin = new AdminUser
        {
            First_name = registerDto.First_name,
            Last_name = registerDto.Last_name,
            Email = registerDto.Email,
            Password = passwordHash,
            PhoneNumber = registerDto.PhoneNumber,
            State = registerDto.State,
            Country = registerDto.Country,
            Address = registerDto.Address,
            Role = registerDto.Role,
        };

        _context.Admins.Add(admin);
        await _context.SaveChangesAsync();

        var token = GenerateJwtToken(admin);

        return new AdminCreateResponseDto
        {
            Id = admin.Id,
            First_name = admin.First_name,
            Last_name = admin.Last_name,
            Email = admin.Email,
            Role = admin.Role,
            CreatedAt = admin.CreatedAt,
            PhoneNumber = admin.PhoneNumber,
            Message = "Admin created successfully"
        };
    }

    public async Task<AdminLoginResponseDto> AdminLogin(LoginDto loginDto)
    {
        var admin = await _context.Admins
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Email.ToLower() == loginDto.Email.ToLower()) ?? throw new Exception("Invalid email or password");

        // VERIFY PASSWORD
        if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, admin.Password))
        {
            throw new UnauthorizedAccessException("Invalid password");
        }

        // AUTOMATICALLY ACTIVATE ADMIN ON SUCCESSFUL LOGIN
        admin.IsActive = true;

        // UPDATE LAST LOGIN TIME
        admin.LastLogin = DateTime.UtcNow;
        _context.Admins.Attach(admin);
        _context.Entry(admin).State = EntityState.Modified;

        await _context.SaveChangesAsync();


        // GENERATE JWT TOKEN
        var token = GenerateJwtToken(admin);

        return new AdminLoginResponseDto
        {
            Id = admin.Id,
            First_name = admin.First_name,
            Last_name = admin.Last_name,
            Email = admin.Email,
            Role = admin.Role,
            // CreatedAt = admin.CreatedAt,
            // PhoneNumber = admin.PhoneNumber,
            // AvatarUrl = admin.AvatarUrl,
            // State = admin.State,
            // Country = admin.Country,
            // Address = admin.Address,
            // LastLogin = admin.LastLogin,
            Token = token,
            IsActive = admin.IsActive
        };
    }

    // New method to get admin details for viewing (no token)
    public async Task<AdminViewResponseDto> GetAdminById(Guid adminId)
    {
        var admin = await _context.Admins.FirstOrDefaultAsync(a => a.Id == adminId)
            ?? throw new KeyNotFoundException("Admin not found");

        return new AdminViewResponseDto
        {
            Id = admin.Id,
            First_name = admin.First_name,
            Last_name = admin.Last_name,
            Email = admin.Email,
            PhoneNumber = admin.PhoneNumber,
            State = admin.State,
            Country = admin.Country,
            Address = admin.Address,
            Role = admin.Role,
            CreatedAt = admin.CreatedAt,
            LastLogin = admin.LastLogin,
            IsActive = admin.IsActive
        };
    }

    private string GenerateJwtToken(AdminUser admin)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, admin.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Name, $"{admin.First_name} {admin.Last_name}"),
            new Claim(JwtRegisteredClaimNames.Email, admin.Email),
            new Claim(ClaimTypes.Role, admin.Role),
            new Claim(ClaimTypes.NameIdentifier, admin.Id.ToString()),
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

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
