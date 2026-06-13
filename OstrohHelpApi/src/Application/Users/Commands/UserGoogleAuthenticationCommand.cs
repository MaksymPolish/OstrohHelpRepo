using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using Application.Services.Interface;
using Application.Users.Exceptions;
using Domain.Users;
using MediatR;
using Microsoft.Extensions.Logging;
using Google.Apis.Auth;

namespace Application.Users.Commands;

public class UserGoogleAuthenticationCommand : IRequest<User>
{
    public string IdToken { get; set; }
}

public class UserGoogleAuthenticationHandler(
    IUserRepository _userRepository, 
    IUserQuery _userQuery,
    ILogger<UserGoogleAuthenticationHandler> _logger,
    IAuthService _authService,
    Microsoft.Extensions.Configuration.IConfiguration _configuration) : IRequestHandler<UserGoogleAuthenticationCommand, User>
{
    public async Task<User> Handle(UserGoogleAuthenticationCommand request, CancellationToken ct)
    {
        (string? googleId, string? email, string? fullName, string? photoUrl) userInfo = (null, null, null, null);

        try
        {
            // Спробуємо Google OAuth токен
            var googlePayload = await ValidateGoogleTokenAsync(request.IdToken, ct);
            userInfo = (
                googleId: googlePayload.Subject,
                email: googlePayload.Email,
                fullName: googlePayload.Name,
                photoUrl: googlePayload.Picture // URL фото з Google
            );
        }
        catch (InvalidGoogleTokenException ex)
        {
            _logger.LogError(ex, "Invalid Google token, failing authentication.");
            throw new Exception($"Google Token Validation Failed: {ex.Message}. Inner details: {ex.InnerException?.Message ?? "No inner details"}");
        }

        if (string.IsNullOrEmpty(userInfo.email))
        {
            _logger.LogError("Email not found in Google token. Make sure you request 'userinfo.email' scope.");
            throw new Exception("Email not found in token. Make sure you request 'userinfo.email' scope from Google.");
        }

        var user = await _userRepository.GetByGoogleIdOrEmailAsync(userInfo.googleId, userInfo.email, ct);

        if (user == null)
        {
            // Визначаємо роль на основі email - HeadOfService для максима, інші - Student
            var roleEnum = userInfo.email == "maksym.polishchuk@oa.edu.ua"
                ? Domain.Users.Roles.RoleEnum.HeadOfService
                : Domain.Users.Roles.RoleEnum.Student;
            var roleGuid = Domain.Users.Roles.Role.GetGuidByEnum(roleEnum);

            user = new User
            {
                Id = Guid.NewGuid(),
                GoogleId = userInfo.googleId,
                Email = userInfo.email!,
                FullName = userInfo.fullName ?? "User", // Мінімальне значення якщо немає імені
                PhotoUrl = userInfo.photoUrl, // Реальна URL фото з Google
                RoleId = roleGuid,
                CreatedAt = DateTime.UtcNow
            };

            await _userRepository.AddAsync(user, ct);
            
            // ВАЖЛИВО: Завантажити користувача з ролью з БД
            var userWithRole = await _userQuery.GetByIdWithRoleAsync(user.Id, ct);
            user = userWithRole.ValueOr(user);
        }
        else
        {
            // Визначаємо нові значення - тільки реальні дані з Google
            var newFullName = userInfo.fullName ?? user.FullName; // Зберігаємо старе якщо нема нового
            var newPhotoUrl = userInfo.photoUrl ?? user.PhotoUrl; // Зберігаємо старе якщо нема нового
            
            // Визначаємо очікувану роль на основі email
            var expectedRoleEnum = userInfo.email == "maksym.polishchuk@oa.edu.ua"
                ? Domain.Users.Roles.RoleEnum.HeadOfService
                : Domain.Users.Roles.RoleEnum.Student;
            var expectedRoleGuid = Domain.Users.Roles.Role.GetGuidByEnum(expectedRoleEnum);
            
            // Перевіряємо, чи змінилось щось
            bool hasChanges = false;
            if (user.FullName != newFullName)
            {
                user.FullName = newFullName;
                hasChanges = true;
            }
            
            if (user.PhotoUrl != newPhotoUrl)
            {
                user.PhotoUrl = newPhotoUrl;
                hasChanges = true;
            }
            
            // Перевіряємо, чи потрібно оновити роль
            if (user.RoleId != expectedRoleGuid)
            {
                user.RoleId = expectedRoleGuid;
                hasChanges = true;
                _logger.LogInformation("Updated user {Email} role to {Role}", userInfo.email, expectedRoleEnum);
            }
            
            // Оновлюємо ТІЛЬКИ якщо є зміни
            if (hasChanges)
            {
                await _userRepository.UpdateAsync(user, ct);
            }
            
            // ВАЖЛИВО: Завантажити користувача з ролью з БД
            var userWithRole = await _userQuery.GetByIdWithRoleAsync(user.Id, ct);
            user = userWithRole.ValueOr(user);
        }

        _logger.LogInformation("User authenticated successfully: {Email}", userInfo.email);

        var jwtToken = _authService.GenerateJwtToken(user);
        var refreshToken = _authService.GenerateRefreshToken();

        return user;
    }

    private async Task<GoogleJsonWebSignature.Payload> ValidateGoogleTokenAsync(string idToken, CancellationToken ct)
    {
        try
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings();
            var clientId = (_configuration["GOOGLE_CLIENT_ID"] ?? Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID"))?.Trim();

            // Для дебагу: читаємо реальні Audience з токена
            var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            if (handler.CanReadToken(idToken))
            {
                var jwt = handler.ReadJwtToken(idToken);
                _logger.LogInformation("Real Token Audiences: {Audiences}", string.Join(", ", jwt.Audiences));
                _logger.LogInformation("Expected ClientId: {ClientId}", clientId);
                _logger.LogInformation("Token Valid From: {ValidFrom:O} UTC, Valid To: {ValidTo:O} UTC. Backend Current Time: {UtcNow:O} UTC", 
                    jwt.ValidFrom, jwt.ValidTo, DateTime.UtcNow);
            }

            if (!string.IsNullOrEmpty(clientId))
            {
                settings.Audience = new[] { 
                    clientId,
                    "930738005847-7q4c0aavnniv1mncbg4lsgbfkkkbnsht.apps.googleusercontent.com",
                    "930738005847-sab62t3hnkmu4ihrfa63rt86msin9c60.apps.googleusercontent.com",
                    "930738005847-kols9m8s95kpg9her0cekfn38u0bvi6q.apps.googleusercontent.com" // Web frontend Client ID
                };
            }
            


            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
            return payload;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Invalid Google token");
            throw new InvalidGoogleTokenException($"Failed to validate Google token: {ex.Message}");
        }
    }
}