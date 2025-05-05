using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using Application.Users.Exceptions;
using Domain.Users;
using MediatR;
using Microsoft.Extensions.Logging;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Auth;

namespace Application.Users.Commands;

public class UserGoogleAuthenticationCommand : IRequest<User>
{
    public string IdToken { get; set; }
}

public class UserGoogleAuthenticationHandler(IUserRepository _userRepository, IRoleQuery _roleQuery, ILogger<UserGoogleAuthenticationHandler> _logger) : IRequestHandler<UserGoogleAuthenticationCommand, User>
{
    public async Task<User> Handle(UserGoogleAuthenticationCommand request, CancellationToken ct)
    {
        var payload = await ValidateGoogleTokenAsync(request.IdToken, ct);

        if (payload == null)
            throw new InvalidGoogleTokenException();

        var googleId = payload.Subject;
        var email = payload.Email;
        var firstName = payload.Name;
        var lastName = payload.FamilyName;

        var user = await _userRepository.GetByGoogleIdOrEmailAsync(googleId, email, ct);

        if (user == null)
        {
            // 🔍 Отримуємо роль "student"
            var studentRoleId = await _roleQuery.GetRoleIdByNameAsync("Студент", ct);
            if (studentRoleId is null)
                throw new RoleNotFoundException("Студент");

            user = new User
            {
                Id = UserId.New(),
                GoogleId = googleId,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                RoleId = studentRoleId,
                CreatedAt = DateTime.UtcNow
            };

            await _userRepository.AddAsync(user, ct);
        }
        else
        {
            user.FirstName = firstName;
            user.LastName = lastName;
            await _userRepository.UpdateAsync(user, ct);
        }

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
}