using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using WSFBackendApi.Data;
using WSFBackendApi.Services;

var builder = WebApplication.CreateBuilder(args);

// JWT KEY GENERATION AND VALIDATION
string jwtKey = EnsureJwtKey(builder.Configuration);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// INCLUDE DATABASE CONTEXT SERVICE
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseMySql(
    builder.Configuration.GetConnectionString("DefaultConnection"),
    ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))
));

// AUTHENTICATION SERVICE REGISTRATION
builder.Services.AddScoped<AuthService>();

// CONFIGURE JWT AUTHENTICATION
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(5)
        };
    });

// ADD CORS POLICY FOR REACT NATIVE
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactNative", builder =>
    {
        builder.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

static string EnsureJwtKey(IConfiguration configuration)
{
    var jwtKey = configuration["Jwt:Key"];
    if (string.IsNullOrEmpty(jwtKey))
    {
        // jwtKey = Guid.NewGuid().ToString();
        jwtKey = GenerateSecureJwtKey();

        // Console.WriteLine("Generated New JWT Key. Please update your appsettings.json");
        // Console.WriteLine($"New JWT Key: {jwtKey}");
        configuration["Jwt:Key"] = jwtKey;
    }

    if (jwtKey.Length < 32)
    {
        Console.WriteLine("JWT Key must be at least 32 characters long.");
        Environment.Exit(1);
    }
    return jwtKey;
}

// METHOD TO GENERATE A CRYPTOGRAPHICALLY SECURE KEY
static string GenerateSecureJwtKey()
{
    // METHOD TO GENERATE A 512-BIT (64-BYTE) CRYPTOGRAPHICALLY SECURE JWT KEY
    byte[] key = new byte[64];
    using (var rng = RandomNumberGenerator.Create())
    {
        rng.GetBytes(key);
    }
    return Convert.ToBase64String(key);
}

var app = builder.Build();

// CONFIGURE THE HTTP REQUEST PIPELINE
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowReactNative");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
