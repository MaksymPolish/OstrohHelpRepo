using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using Domain.Conferences.Statuses;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/ConsultationStatus")]
public class ConsultationStatusController(IConsultationStatusQuery _consultationStatusQuery,
    IMediator _mediator) : ControllerBase 
{
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
}