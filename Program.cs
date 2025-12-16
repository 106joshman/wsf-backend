using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.RateLimiting;
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

// CONFIGURE POSTGRESQL DATABASE CONTEXT WITH ENTITY FRAMEWORK
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    string connectionString;

    // First, try to get the full DATABASE_URL (Render's default)
    var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

    if (!string.IsNullOrEmpty(databaseUrl))
    {
        try // Y#&VM..EEAv459%
        {
            // Parse the DATABASE_URL format: postgresql://user:password@host:port/database
            // var uri = new Uri(databaseUrl);

            // // Extract username and password safely
            // var userInfo = uri.UserInfo.Split(':');
            // var username = userInfo.Length > 0 ? userInfo[0] : "";
            // var password = userInfo.Length > 1 ? userInfo[1] : "";

            // // Extract database name (remove leading slash)
            // var database = uri.AbsolutePath.TrimStart('/');

            // // Use default port 5432 if not specified
            // var port = uri.Port > 0 ? uri.Port : 5432;

            // Remove the scheme
            var url = databaseUrl.Replace("postgresql://", "");
            var atSplit = url.Split('@', 2);
            var userPass = atSplit[0].Split(':', 2);
            var hostDb = atSplit[1].Split('/', 2);
            var hostPort = hostDb[0].Split(':', 2);

            var username = userPass[0];
            var password = userPass.Length > 1 ? userPass[1] : "";
            var host = hostPort[0];
            var port = hostPort.Length > 1 ? hostPort[1] : "5432";
            var database = hostDb.Length > 1 ? hostDb[1] : "";

            connectionString = $"Host={host};Port={port};Database={database};Username={username};Password={password};SSL Mode=Require;Trust Server Certificate=true;";
        }
        catch (Exception ex)
        {
           Console.WriteLine($"Error parsing DATABASE_URL: {ex.Message}");
            throw;
        }
    }
    else
    {
        // Fall back to local config (appsettings.json)
        connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("No database connection string found");

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("No database connection string found. Please set DATABASE_URL or configure DefaultConnection.");
        }

    //    Console.WriteLine("Using local connection string from appsettings.json");
    }

    options.UseNpgsql(connectionString);
});

// REGISTER ALL SERVICE
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<AdminAuthService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<LocationService>();
builder.Services.AddScoped<OutlineService>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<PushNotificationSender>();

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
            //    Console.WriteLine($"Authentication failed: {context.Exception.Message}");
                return Task.CompletedTask;
            }
        };
    });

// ADD CORS POLICY FOR REACT NATIVE
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontendClients", builder =>
    {
        builder.SetIsOriginAllowed(origin =>
            {
                var uri = new Uri(origin);
                return uri.Host == "localhost" ||
                       uri.Host.StartsWith("wsf-admin.netlify.app") ||
                       uri.Host=="http://localhost:3000/";
            })
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// CONFIGURE RATE LIMITING
builder.Services.AddRateLimiter(options =>
{
    // REGISTER ENDPOINT -- PREVENT BOT SIGN UPS
    options.AddPolicy("registerPolicy", ctx =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: partition => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 3, // MAX 3 SIGN UP REQUESTS PER IP PER MINUTE
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }
        )
    );

    // LOGIN ENDPOINT - EXTRA IP-LEVEL PROTECTION
    options.AddPolicy("loginPolicy", ctx =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: partition => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10, // 10 LOGIN ATTEMPT PER IP PER MINUTE
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst
            }
        )
    );

    // GET LOCATIONS ENDPOINT - PROTECT FROM ABUSE / BOT CALLS
    options.AddPolicy("getLocationsPolicy", ctx =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: ctx.User?.Identity?.Name ?? "anonymous",
            factory: partition => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 30, // 30 REQUESTS PER USER PER MINUTE
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }
        )
    );
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
    //    Console.WriteLine("JWT Key must be at least 32 characters long.");
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
using var scope = app.Services.CreateScope();
var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
try
{
    dbContext.Database.Migrate();

    // SEED THE SUPER ADMIN USER TO DATABASE
    await SuperAdminSeeder.SeedAsync(app.Services);
    await MultipleAdminsSeeder.SeedAsync(app.Services);
}
catch (Exception ex)
{
    Console.WriteLine($"Migration failed: {ex.Message}");
    // Don't throw in production - let app start even if migration fails
    // You can then fix and redeploy
}


// CONFIGURE THE HTTP REQUEST PIPELINE
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
});
}

// app.UseHttpsRedirection();
app.UseCors("AllowFrontendClients");
app.UseRateLimiter();
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
