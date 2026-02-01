using Api.Dtos;
using Application.Common.Interfaces.Queries;
using Application.Services.Interface;
using Application.Users.Commands;
using AutoMapper;
using Domain.Users;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(
    IAuthService _authService, 
    IMapper _mapper, 
    IMediator _mediator, 
    IUserQuery _userQuery) : ControllerBase
{
    [AllowAnonymous]
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
        var userId = new UserId(id);
        // ✅ Один запит до БД з Include замість N+1
        var userOption = await _userQuery.GetByIdWithRoleAsync(userId, ct);

        return userOption.Match<IActionResult>(
            user =>
            {
                var dto = _mapper.Map<UserDto>(user);
                dto.RoleName = user.Role?.Name ?? "Невідома роль";
                return Ok(dto);
            },
            () => NotFound(new { Message = $"User with ID '{id}' was not found" })
        );
    }
    
    [HttpGet("get-by-email")]
    public async Task<IActionResult> GetByEmail([FromQuery] string email, CancellationToken ct)
    {
        // ✅ Один запит до БД з Include замість N+1
        var userOption = await _userQuery.GetByEmailWithRoleAsync(email, ct);

        return userOption.Match<IActionResult>(
            user =>
            {
                var dto = _mapper.Map<UserDto>(user);
                dto.RoleName = user.Role?.Name ?? "Невідома роль";
                return Ok(dto);
            },
            () => NotFound(new { Message = $"User with email '{email}' was not found" })
        );
    }

    [HttpGet("all")]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        // ✅ Один запит до БД з Include замість N+1
        var users = await _userQuery.GetAllWithRolesAsync(ct);

        var dtos = users.Select(user =>
        {
            var dto = _mapper.Map<UserDto>(user);
            dto.RoleName = user.Role?.Name ?? "Невідома роль";
            return dto;
        }).ToList();

        return Ok(dtos);
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
    
    [HttpPut("User-Role-Update")]
    public async Task<IActionResult> Update([FromBody] UpdateUserCommand command, CancellationToken ct)
    {
        await _mediator.Send(command, ct);
        return NoContent(); 
    }
    
    
    
    
}