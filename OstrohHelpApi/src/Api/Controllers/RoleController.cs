using Application.Roles.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RoleController(IMediator _mediator) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRoleCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct); // ❌ NullReference тут
        return CreatedAtAction(nameof(Create), new { id = result.Id }, result);
    }
    
    
}