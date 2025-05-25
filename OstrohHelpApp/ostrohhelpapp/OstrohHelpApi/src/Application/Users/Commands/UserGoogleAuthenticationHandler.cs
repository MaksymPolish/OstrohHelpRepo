using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Interfaces;
using Application.Users.Exceptions;
using Google.Apis.Auth;
using MediatR;
using Microsoft.Extensions.Configuration;

namespace Application.Users.Commands;

public class UserGoogleAuthenticationCommand : IRequest<string>
{
    public string IdToken { get; set; }
}

public class UserGoogleAuthenticationHandler : IRequestHandler<UserGoogleAuthenticationCommand, string>
{
    private readonly IConfiguration _configuration;
    private readonly IUserService _userService;

    public UserGoogleAuthenticationHandler(IConfiguration configuration, IUserService userService)
    {
        _configuration = configuration;
        _userService = userService;
    }

    public async Task<string> Handle(UserGoogleAuthenticationCommand request, CancellationToken ct)
    {
        var clientId = _configuration["GoogleAuth:ClientId"];
        var payload = await ValidateGoogleTokenAsync(request.IdToken, clientId, ct);
        
        // Create or get user based on Google payload
        var user = await _userService.GetOrCreateUserAsync(payload.Email, payload.Name, payload.Picture, ct);
        
        // Generate JWT token for the user
        return await _userService.GenerateJwtTokenAsync(user, ct);
    }

    private async Task<GoogleJsonWebSignature.Payload> ValidateGoogleTokenAsync(string idToken, string audience, CancellationToken ct)
    {
        try
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { audience }
            };

            return await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
        }
        catch (InvalidJwtException ex)
        {
            throw new InvalidGoogleTokenException("Provided Google token is invalid or expired.", ex);
        }
    }
} 