using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using Application.Roles.Commands;
using Domain.Users.Roles;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RoleController(IMediator _mediator, IRoleQuery _roleQuery, IRoleRepository _roleRepository) : ControllerBase
{
    [HttpPost("Create-Role")]
    public async Task<IActionResult> Create([FromBody] CreateRoleCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return CreatedAtAction(nameof(Create), new { id = result.Id }, result);
    }
    
    [HttpGet("Get-All-Roles")]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var roles = await _roleQuery.GetAllAsync(ct);
        return Ok(roles);
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken ct)
    {
        var roleId = new RoleId(id);
        var roleOption = await _roleQuery.GetByIdAsync(roleId, ct);

        return roleOption.Match<IActionResult>(
            r => Ok(r),
            () => NotFound(new { Message = $"Role with ID '{id}' was not found" })
        );
    }
    
    [HttpPut("Update-Role")]
    public async Task<IActionResult> Update([FromBody] UpdateRoleCommand command, CancellationToken ct)
    {
        await _mediator.Send(command, ct);
        return NoContent();
    }
    
    [HttpDelete("Delete-Role")]
    public async Task<IActionResult> Delete([FromBody] Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new DeleteRoleCommand { Id = id }, ct);
        return result.Match<IActionResult>(
            role => NoContent(),
            exception => BadRequest(new
            {
                Error = exception.Message,
                RoleId = exception.RoleId.ToString()
            })
        );
        
    }
    
    
}