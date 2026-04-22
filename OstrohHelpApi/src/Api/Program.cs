//using Api.Modules;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Application;
using Infrastructure;
using Infrastructure.Jobs;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.FileProviders;
using DotNetEnv;
using Hangfire;
using Hangfire.PostgreSql;
using Api.Services;

using Api.Middleware;
using FluentValidation;
using FluentValidation.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Load .env file from project root (go up from src/Api to root)
var envPath = Path.Combine(builder.Environment.ContentRootPath, ".env");

// If not found, try going up two levels (from src/Api to root)
if (!File.Exists(envPath))
{
    envPath = Path.Combine(builder.Environment.ContentRootPath, "../../.env");
    envPath = Path.GetFullPath(envPath); // Normalize the path
}

if (File.Exists(envPath))
{
    DotNetEnv.Env.Load(envPath);
}

// Configure connection string from environment variables (.env has priority)
var databaseUrl = DotNetEnv.Env.GetString("DATABASE_URL");
if (!string.IsNullOrEmpty(databaseUrl))
{
    builder.Configuration["ConnectionStrings:Default"] = databaseUrl;
    Console.WriteLine("✓ Database connection string loaded from .env");
}
else
{
    Console.WriteLine("ℹ Database connection string from appsettings.json");
}

// Cloudinary service registration
builder.Services.AddSingleton<Api.Services.CloudinaryService>();
builder.Services.AddSingleton<Application.Common.Interfaces.Services.IFileUploadService>(
    sp => sp.GetRequiredService<Api.Services.CloudinaryService>());
builder.Services.AddSingleton<Application.Common.Interfaces.Services.IPreviewGenerationService>(
    sp => sp.GetRequiredService<Api.Services.CloudinaryService>());

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

// Swagger з підтримкою JWT авторизації
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "OstrohHelp API",
        Version = "v1",
        Description = "API для системи психологічної допомоги"
    });
    
    // Налаштування JWT авторизації в Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Введіть JWT токен.\n\nПриклад: eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
    });
    
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Register custom JSON converter for byte[] → Base64 serialization
        options.JsonSerializerOptions.Converters.Add(new Infrastructure.Serialization.ByteArrayToBase64Converter());
    });
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Application.ApplicationAssemblyMarker>();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

// Add SignalR for real-time chat
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
})
.AddJsonProtocol(options =>
{
    // Register custom JSON converter for byte[] → Base64 serialization
    // This ensures encrypted data (EncryptedContent, Iv, AuthTag) is transmitted as Base64 strings
    options.PayloadSerializerOptions.Converters.Add(new Infrastructure.Serialization.ByteArrayToBase64Converter());
});

// Add CORS configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins(
            "http://localhost:3000",      // React
            "http://localhost:5000",      // Test HTML
            "http://127.0.0.1:5500",      // Local static server (e.g., Live Server)
            "http://localhost:5173",      // Vite
            "http://localhost:4200",      // Angular
            "http://localhost:7000",      // Self (for testing)
            "https://localhost:7000",
            "http://localhost:7001",
            "https://localhost:7001"
        )
        .AllowAnyMethod()          // GET, POST, PUT, DELETE, OPTIONS, etc.
        .AllowAnyHeader()          // Any headers
        .AllowCredentials()        // Important for SignalR and auth
        .WithExposedHeaders("Content-Disposition", "X-Pagination"); // Expose custom headers if needed
    });
});

// JWT Bearer Authentication configuration
// Read from .env first (priority), then fallback to appsettings.json
var jwtSecret = DotNetEnv.Env.GetString("JWT_SECRET") ?? builder.Configuration["Jwt:Secret"];
var jwtIssuer = DotNetEnv.Env.GetString("JWT_ISSUER") ?? builder.Configuration["Jwt:Issuer"];
var jwtAudience = DotNetEnv.Env.GetString("JWT_AUDIENCE") ?? builder.Configuration["Jwt:Audience"];

if (!string.IsNullOrEmpty(DotNetEnv.Env.GetString("JWT_SECRET")))
{
    Console.WriteLine("✓ JWT credentials loaded from .env file");
}
else
{
    Console.WriteLine("ℹ JWT credentials loaded from appsettings.json (or appsettings.Development.json)");
}

// Важливо: Вимкнути автоматичне перейменування claims
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret!)),
            ClockSkew = TimeSpan.Zero // Без затримки при перевірці терміну дії токена
        };
        
        // SignalR support - читання токену з query string
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;

                // Дозволити access_token для negotiate + WebSocket на /hubs/chat
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs/chat"))
                {
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            }
        };
    })
    .AddCookie("Cookies")
    .AddGoogle("Google", options =>
    {
        // Зчитування ClientId та ClientSecret з .env файлу (пріоритет) або google-auth.json
        var googleClientId = DotNetEnv.Env.GetString("GOOGLE_CLIENT_ID");
        var googleClientSecret = DotNetEnv.Env.GetString("GOOGLE_CLIENT_SECRET");

        // Використовуємо значення з .env
        options.ClientId = googleClientId;
        options.ClientSecret = googleClientSecret;
        Console.WriteLine("✓ Google OAuth credentials loaded from .env");

        options.SignInScheme = "Cookies";
        options.CallbackPath = "/auth/callback"; // шлях, куди повертається Google
    });

// Authorization policies
builder.Services.AddAuthorization(options =>
{
    // Політика для студентів (всі автентифіковані користувачі)
    options.AddPolicy("RequireAuthenticated", policy => 
        policy.RequireAuthenticatedUser());
    
    // Політика тільки для студентів
    options.AddPolicy("RequireStudent", policy => 
        policy.RequireRole("Student", "Psychologist", "HeadOfService"));
    
    // Політика для психологів та керівників
    options.AddPolicy("RequirePsychologist", policy => 
        policy.RequireRole("Psychologist", "HeadOfService"));
    
    // Політика тільки для керівника служби
    options.AddPolicy("RequireHeadOfService", policy => 
        policy.RequireRole("HeadOfService"));
    
    // Політика для персоналу (психологи та керівники)
    options.AddPolicy("RequireStaff", policy => 
        policy.RequireRole("Psychologist", "HeadOfService"));
});

// Configure AutoMapper with license key from .env
var autoMapperLicenseKey = DotNetEnv.Env.GetString("AUTOMAPPER_LICENSE_KEY");

if (!string.IsNullOrEmpty(autoMapperLicenseKey))
{
    Console.WriteLine("✓ AutoMapper License Key loaded from .env");
}
else
{
    Console.WriteLine("⚠ AutoMapper License Key not found in .env - using open-source version");
}

// AutoMapper configuration - scan all assemblies for profiles
builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddMaps(typeof(Program).Assembly, typeof(Application.ApplicationAssemblyMarker).Assembly);
});

// Ініціалізація Firebase Admin SDK
var firebaseProjectId = DotNetEnv.Env.GetString("FIREBASE_PROJECT_ID");
var firebasePrivateKey = DotNetEnv.Env.GetString("FIREBASE_PRIVATE_KEY");
var firebaseClientEmail = DotNetEnv.Env.GetString("FIREBASE_CLIENT_EMAIL");

if (!string.IsNullOrEmpty(firebaseProjectId) && !string.IsNullOrEmpty(firebasePrivateKey) && !string.IsNullOrEmpty(firebaseClientEmail))
{
    // Використовуємо значення з .env
    // Примітка: Firebase потребує специфічного формату приватного ключа
    // Переконайтеся, що FIREBASE_PRIVATE_KEY містить правильно форматований PEM ключ
    Console.WriteLine("✓ Firebase credentials detected in .env - will use these for authentication");
}
else
{
    // Fallback - читаємо з файлу ostrohhelpapp.json
    var firebaseJsonPath = Path.Combine(builder.Environment.ContentRootPath, "ostrohhelpapp.json");
    if (File.Exists(firebaseJsonPath))
    {
        try
        {
            FirebaseApp.Create(new AppOptions()
            {
                Credential = GoogleCredential.FromFile(firebaseJsonPath)
            });
            Console.WriteLine("✓ Firebase credentials loaded from ostrohhelpapp.json");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠ Firebase initialization from file failed: {ex.Message}");
        }
    }
    else
    {
        Console.WriteLine("⚠ Firebase credentials not found in .env or ostrohhelpapp.json file");
    }
}

// Configure Hangfire for background job processing
var connectionString = builder.Configuration.GetConnectionString("Default");
builder.Services.AddHangfire((config) =>
{
    config
        .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UsePostgreSqlStorage(options => 
            options.UseNpgsqlConnection(connectionString));
});
builder.Services.AddHangfireServer();

// Add Memory Cache for rate limiting
builder.Services.AddMemoryCache();

// Presence tracking for SignalR online/offline status across multiple devices.
builder.Services.AddSingleton<IPresenceTracker, PresenceTracker>();

// Register Rate Limiting Service (MUST be Singleton, used in Middleware pipeline at root level)
builder.Services.AddSingleton<Application.Common.Services.IRateLimitingService, Application.Common.Services.RateLimitingService>();

var app = builder.Build();

// Глобальна обробка помилок
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Enable CORS (MUST be before Authentication and Authorization)
app.UseCors("AllowAll");

// Handle preflight OPTIONS requests for CORS
app.Use(async (context, next) =>
{
    if (context.Request.Method == "OPTIONS")
    {
        context.Response.StatusCode = 200;
        await context.Response.CompleteAsync();
        return;
    }
    await next.Invoke();
});

app.UseAuthentication(); 
app.UseAuthorization();

// Apply rate limiting middleware
app.UseRateLimiting();

// Configure Hangfire Dashboard (optional - for monitoring)
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    // Restrict dashboard access to development only
    Authorization = new[] { new HangfireAuthorizationFilter(app.Environment) }
});

app.MapControllers();

// Map SignalR Hub
app.MapHub<Api.Hubs.ChatHub>("/hubs/chat");

// Schedule recurring Hangfire jobs
RecurringJob.AddOrUpdate<OrphanedAttachmentCleanupJob>(
    "orphaned-attachment-cleanup",
    job => job.ExecuteAsync(CancellationToken.None),
    Cron.Daily(2) // Execute daily at 2:00 AM UTC
);

// Universal static files setup for media
var mediaPath = Path.Combine(builder.Environment.ContentRootPath, "data/media");
if (!Directory.Exists(mediaPath))
{
    Directory.CreateDirectory(mediaPath);
}
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(mediaPath),
    RequestPath = "/media"
});

app.Run();

public partial class Program;