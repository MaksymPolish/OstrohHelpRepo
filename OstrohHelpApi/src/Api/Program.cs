//using Api.Modules;
using Application;
using Infrastructure;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.Cookies;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
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

var imagesPath = Path.Combine(builder.Environment.ContentRootPath, "data/images");

if (!Directory.Exists(imagesPath))
{
    Directory.CreateDirectory(imagesPath);
    
    var containersPath = Path.Combine(imagesPath, "containers");
    if (!Directory.Exists(containersPath))
    {
        Directory.CreateDirectory(containersPath);
    }
    
    var productsPath = Path.Combine(imagesPath, "products");
    if (!Directory.Exists(productsPath))
    {
        Directory.CreateDirectory(productsPath);
    }
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(imagesPath),
    RequestPath = "/images"
});



app.Run();

public partial class Program;