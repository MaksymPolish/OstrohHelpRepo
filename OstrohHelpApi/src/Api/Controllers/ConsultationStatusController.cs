using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using Application.ConsultationStatus.Commands;
using Domain.Conferences.Statuses;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/ConsultationStatus")]
public class ConsultationStatusController(IConsultationStatusQuery _consultationStatusQuery,
    IMediator _mediator) : ControllerBase 
{
    //Add
    [HttpPost("Add-ConsultationStatus")]
    public async Task<IActionResult> Add([FromBody] CreateConsultationStatusCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        
        return result.Match<IActionResult>(
            status => CreatedAtAction(nameof(Add), new { id = status.Id }, status),
            ex => BadRequest(new { Error = ex.Message })
        );
    }
    
    //GetAll
    [HttpGet("Get-All-ConsultationStatuses")]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var roles = await _consultationStatusQuery.GetAllAsync(ct);
        return Ok(roles);
    }
    
    //GetById
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken ct)
    {
        var roleId = new ConsultationStatusesId(id);
        var roleOption = await _consultationStatusQuery.GetByIdAsync(roleId, ct);
        
        return roleOption.Match<IActionResult>(
            r => Ok(r),
            () => NotFound(new { Message = $"Role with ID '{id}' was not found" })
        );
    }
    
    //Update
    [HttpPut("Update-ConsultationStatus")]
    public async Task<IActionResult> Update([FromBody] UpdateConsultationStatusCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);

        return result.Match<IActionResult>(
            _ => NoContent(),
            ex => BadRequest(new { Error = ex.Message })
        );
    }
    //Delete
    [HttpDelete("Delete-ConsultationStatus")]
    public async Task<IActionResult> Delete([FromBody]Guid id, CancellationToken ct)
    {
        var command = new DeleteConsultationStatusCommand(id);
        var result = await _mediator.Send(command, ct);

        return result.Match<IActionResult>(
            _ => NoContent(),
            ex => BadRequest(new { Error = ex.Message })
        );
    }
}