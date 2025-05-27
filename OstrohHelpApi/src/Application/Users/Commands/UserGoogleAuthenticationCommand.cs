﻿using Application.Common.Interfaces.Queries;
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
        (string? googleId, string email, string? fullName) userInfo = (null, null!, null);
        
        try
        {
            // Спробуємо Google OAuth токен
            var googlePayload = await ValidateGoogleTokenAsync(request.IdToken, ct);
            
            userInfo = (
                googleId: googlePayload.Subject,
                email: googlePayload.Email,
                fullName: googlePayload.Name
            );
            
        }
        catch (InvalidGoogleTokenException)
        {
            // Якщо не Google — спробуємо Firebase
            var firebaseUser = await ValidateFirebaseTokenAsync(request.IdToken, ct);
            userInfo = (
                googleId: firebaseUser.Uid,
                email: firebaseUser.Claims.GetValueOrDefault("email")?.ToString()
                       ?? throw new InvalidFirebaseTokenException(),
                fullName: firebaseUser.Claims.GetValueOrDefault("name")?.ToString()
            );
        }

        var user = await _userRepository.GetByGoogleIdOrEmailAsync(userInfo.googleId, userInfo.email, ct);

        if (user == null)
        {
            var studentRoleId = await _roleQuery.GetRoleIdByNameAsync("Студент", ct);
            if (studentRoleId is null)
                throw new RoleNotFoundException("Студент");

            user = new User
            {
                Id = UserId.New(),
                GoogleId = userInfo.googleId,
                Email = userInfo.email,
                FullName = userInfo.fullName,
                RoleId = studentRoleId,
                CreatedAt = DateTime.UtcNow
            };

            await _userRepository.AddAsync(user, ct);
        }
        else
        {
            user.FullName = userInfo.fullName;
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