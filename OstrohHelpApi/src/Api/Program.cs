//using Api.Modules;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Application;
using Infrastructure;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.FileProviders;

using Api.Middleware;
using FluentValidation;
using FluentValidation.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
// Cloudinary service registration
builder.Services.AddSingleton<Api.Services.CloudinaryService>();

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

builder.Services.AddControllers();
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
});

// Add CORS configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins(
            "http://localhost:3000",      // React
            "http://localhost:5000",      // Test HTML
            "http://localhost:5173",      // Vite
            "http://localhost:4200",      // Angular
            "http://localhost:7000",      // Self (for testing)
            "https://localhost:7000",
            "http://localhost:7001",
            "https://localhost:7001"
        )
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials(); // Важливо для SignalR
    });
});

// JWT Bearer Authentication configuration
var jwtSecret = builder.Configuration["Jwt:Secret"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

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
        
        // SignalR WebSocket support - читання токену з query string
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                
                // Якщо це WebSocket з'єднання на SignalR Hub
                if (!string.IsNullOrEmpty(accessToken) &&
                    context.HttpContext.WebSockets.IsWebSocketRequest)
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
        // Зчитування ClientId та ClientSecret з google-auth.json
        var googleAuthPath = Path.Combine(builder.Environment.ContentRootPath, "google-auth.json");
        if (File.Exists(googleAuthPath))
        {
            var googleAuthJson = File.ReadAllText(googleAuthPath);
            dynamic googleAuth = Newtonsoft.Json.JsonConvert.DeserializeObject(googleAuthJson);
            options.ClientId = googleAuth.ClientId;
            options.ClientSecret = googleAuth.ClientSecret;
        }
        else
        {
            throw new Exception($"google-auth.json not found at {googleAuthPath}");
        }
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

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// Ініціалізація Firebase Admin SDK (тільки якщо файл існує)
var firebaseJsonPath = Path.Combine(builder.Environment.ContentRootPath, "ostrohhelpapp.json");
if (File.Exists(firebaseJsonPath))
{
    FirebaseApp.Create(new AppOptions()
    {
        Credential = GoogleCredential.FromFile(firebaseJsonPath)
    });
}

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

// Enable CORS
app.UseCors("AllowAll");

app.UseAuthentication(); 
app.UseAuthorization();

app.MapControllers();

// Map SignalR Hub
app.MapHub<Api.Hubs.ChatHub>("/hubs/chat");

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