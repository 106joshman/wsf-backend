using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using WSFBackendApi.Data;
using WSFBackendApi.Middleware;

using WSFBackendApi.Seeders;
using WSFBackendApi.Services;

var builder = WebApplication.CreateBuilder(args);

// JWT KEY GENERATION AND VALIDATION
string jwtKey = EnsureJwtKey(builder.Configuration);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // Your normal Swagger config
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });

    // Add the JWT security scheme
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    // Apply the scheme globally to all operations
    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2", // doesn't affect functionality much
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    string connectionString;

    // First, try to get the full DATABASE_URL (Render's default)
    var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

    if (!string.IsNullOrEmpty(databaseUrl))
    {
        // Parse the DATABASE_URL format: postgresql://user:password@host:port/database
        var uri = new Uri(databaseUrl);

        connectionString =
            $"Host={uri.Host};" +
            $"Port={uri.Port};" +
            $"Database={uri.AbsolutePath.TrimStart('/')};" +
            $"Username={uri.UserInfo.Split(':')[0]};" +
            $"Password={uri.UserInfo.Split(':')[1]};" +
            "SSL Mode=Require;" +
            "Trust Server Certificate=true;";
    }
    else
    {
        // Try individual environment variables
        var host = Environment.GetEnvironmentVariable("POSTGRES_HOST");
        var port = Environment.GetEnvironmentVariable("POSTGRES_PORT");
        var db = Environment.GetEnvironmentVariable("POSTGRES_DB");
        var user = Environment.GetEnvironmentVariable("POSTGRES_USER");
        var password = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD");

        if (!string.IsNullOrEmpty(host) && !string.IsNullOrEmpty(db))
        {
            connectionString =
                $"Host={host};" +
                $"Port={port ?? "5432"};" +
                $"Database={db};" +
                $"Username={user};" +
                $"Password={password};" +
                "SSL Mode=Require;" +
                "Trust Server Certificate=true;";
        }
        else
        {
            // Fall back to local config (appsettings.json)
            connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("No database connection string found");
        }
    }

    options.UseNpgsql(connectionString);
});

// REGISTER ALL SERVICE
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<AdminAuthService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<LocationService>();
builder.Services.AddScoped<OutlineService>();

// CONFIGURE JWT AUTHENTICATION
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuer = true,
            RoleClaimType = ClaimTypes.Role,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(5) // ALLOW 5 MINUTES CLOCK SKEW AFTER TOKEN EXPIRES
        };

        // Add debugging for token validation
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                // Console.WriteLine("Token successfully validated");
                return Task.CompletedTask;
            },
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"Authentication failed: {context.Exception.Message}");
                return Task.CompletedTask;
            }
        };
    });

// ADD CORS POLICY FOR REACT NATIVE
builder.Services.AddCors(options =>
{
    // options.AddPolicy("AllowReactNative", builder =>
    // {
    //     builder.AllowAnyOrigin()
    //         .AllowAnyMethod()
    //         .AllowAnyHeader();
    // });

    options.AddPolicy("AllowFrontendClients", builder =>
    {
        builder.SetIsOriginAllowed(origin =>
            {
                var uri = new Uri(origin);
                return uri.Host == "localhost" ||
                       uri.Host == "127.0.0.1" ||
                       uri.Host.StartsWith("192.168.") ||
                       uri.Host.StartsWith("10.") ||
                       uri.Host.StartsWith("172.") ||
                       uri.Host=="http://localhost:3000/";
            })
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
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

// Auto-apply migrations on startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    try
    {
        Console.WriteLine("Checking and applying database migrations...");
        dbContext.Database.Migrate();
        Console.WriteLine("Database is up to date!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Migration failed: {ex.Message}");
        // Don't throw in production - let app start even if migration fails
        // You can then fix and redeploy
    }
}

// SEED THE SUPER ADMIN USER TO DATABASE
await SuperAdminSeeder.SeedAsync(app.Services);
await MultipleAdminsSeeder.SeedAsync(app.Services);

// CONFIGURE THE HTTP REQUEST PIPELINE
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
});
}

app.UseHttpsRedirection();
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.UseCors("AllowFrontendClients");
app.MapControllers();

app.Run();
