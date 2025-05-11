using Api.Dtos;
using Application.Common.Interfaces.Queries;
using Application.Services.Interface;
using Application.Users.Commands;
using Domain.Users;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService _authService, IMediator _mediator, IUserQuery _userQuery) : ControllerBase
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
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken ct)
    {
        var roleId = new UserId(id);
        var roleOption = await _userQuery.GetByIdAsync(roleId, ct);

        return roleOption.Match<IActionResult>(
            r => Ok(r),
            () => NotFound(new { Message = $"Role with ID '{id}' was not found" })
        );
    }
    
    [HttpGet("all")]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var users = await _userQuery.GetAllAsync(ct);
        
        return Ok(users);
    }

    [HttpDelete("User-Delete")]
    public async Task<IActionResult> Delete([FromBody] Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new DeleteUserCommand { UserId = id }, ct);

        return result.Match<IActionResult>(
            user => NoContent(),
            exception => BadRequest(new
            {
                Error = exception.Message,
                UserId = exception.UserId.ToString()
            })
        );
    }

    [HttpPut("User-course")]
    public async Task<IActionResult> Update_Course([FromBody] AddUserCourseCommand command, CancellationToken ct)
    {
        await _mediator.Send(command, ct);
        return NoContent(); 
    }
    
    
}