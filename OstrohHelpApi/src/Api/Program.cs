//using Api.Modules;
using Application;
using Infrastructure;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.Cookies;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.FileProviders;

using Api.Middleware;
using FluentValidation;
using FluentValidation.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Application.ApplicationAssemblyMarker>();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

// Add CORS configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});
//builder.Services.SetupServices();

//Google authentication configuration
builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = "Cookies";
        options.DefaultChallengeScheme = "Google";
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

app.UseAuthentication(); 
app.UseAuthorization();

//await app.InitialiseDb();
app.MapControllers();





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