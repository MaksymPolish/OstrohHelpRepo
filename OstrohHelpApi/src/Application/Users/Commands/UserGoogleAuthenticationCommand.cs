using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using Application.Services.Interface;
using Application.Users.Exceptions;
using Domain.Users;
using FirebaseAdmin;
using FirebaseAdmin.Auth;
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
    IRoleQuery _roleQuery, 
    ILogger<UserGoogleAuthenticationHandler> _logger,
    IAuthService _authService) : IRequestHandler<UserGoogleAuthenticationCommand, User>
{
    private readonly FirebaseAuth _firebaseAuth = FirebaseAuth.DefaultInstance;
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
        catch (InvalidGoogleTokenException)
        {
            // Якщо не Google — спробуємо Firebase
            var firebaseUser = await ValidateFirebaseTokenAsync(request.IdToken, ct);
            userInfo = (
                googleId: firebaseUser.Uid,
                email: firebaseUser.Claims.GetValueOrDefault("email")?.ToString(),
                fullName: firebaseUser.Claims.GetValueOrDefault("name")?.ToString(),
                photoUrl: firebaseUser.Claims.GetValueOrDefault("picture")?.ToString() // URL фото з Firebase
            );
        }

        if (string.IsNullOrEmpty(userInfo.email))
        {
            _logger.LogError("Email not found in Google/Firebase token. Make sure you request 'userinfo.email' scope.");
            throw new Exception("Email not found in token. Make sure you request 'userinfo.email' scope from Google.");
        }

        var user = await _userRepository.GetByGoogleIdOrEmailAsync(userInfo.googleId, userInfo.email, ct);

        if (user == null)
        {
            // Використовуємо enum для ролі Студент
            var studentRoleGuid = Domain.Users.Roles.Role.GetGuidByEnum(Domain.Users.Roles.RoleEnum.Student);
            var studentRoleId = new Domain.Users.Roles.RoleId(studentRoleGuid);

            user = new User
            {
                Id = UserId.New(),
                GoogleId = userInfo.googleId,
                Email = userInfo.email!,
                FullName = userInfo.fullName,
                PhotoUrl = userInfo.photoUrl, // Зберігаємо URL фото
                RoleId = studentRoleId,
                CreatedAt = DateTime.UtcNow
            };

            await _userRepository.AddAsync(user, ct);
        }
        else
        {
            user.FullName = userInfo.fullName;
            user.PhotoUrl = userInfo.photoUrl; // Оновлюємо фото при кожному логіні
            await _userRepository.UpdateAsync(user, ct);
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
            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
            return payload;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Invalid Google token");
            throw new InvalidGoogleTokenException("Failed to validate Google token.");
        }
    }

    private async Task<dynamic> ValidateFirebaseTokenAsync(string idToken, CancellationToken ct)
    {
        try
        {
            // Firebase перевіряє токен і повертає його claims
            var decodedToken = await _firebaseAuth.VerifyIdTokenAsync(idToken, ct);
            return decodedToken;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Invalid Firebase token");
            throw new InvalidFirebaseTokenException();
        }
    }
}