using Api.Dtos;
using Application.Services.Interface;
using Application.Users.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService _authService, IMediator _mediator) : ControllerBase
{
    [HttpPost("google-login")]
    public async Task<IActionResult> GoogleLogin([FromBody] UserGoogleAuthenticationCommand command, CancellationToken ct)
    {
        var user = await _mediator.Send(command, ct);

        var jwtToken = _authService.GenerateJwtToken(user);
        var refreshToken = _authService.GenerateRefreshToken();

        return Ok(new AuthResultDto
        {
            Id = user.Id.ToString(),
            Email = user.Email,
            FullName = user.FullName,
            RoleId = user.RoleId.ToString(),
            JwtToken = jwtToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        });
    }
}