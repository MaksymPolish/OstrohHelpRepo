using Api.Dtos;
using Application.Users.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    
    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    [HttpPost("google-login")]
    public async Task<IActionResult> GoogleLogin([FromBody] UserGoogleAuthenticationCommand command)
    {
        var user = await _mediator.Send(command);
        var dto = new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            GoogleId = user.GoogleId,
            RoleId = user.RoleId
        };

        return Ok(dto);
    }
}