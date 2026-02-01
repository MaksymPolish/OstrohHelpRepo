using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using Domain.Inventory.Statuses;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/QuestiStatController")]
public class QuestionaryStController(IMediator _mediator, 
    IQuestionnaireStatusQuery _questionnaireStatusQuery) : ControllerBase
{
    [HttpGet("{id}Get-By-Id")]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken ct)
    {
        var statusId = new questionaryStatusId(id);
        var role = await _questionnaireStatusQuery.GetByIdAsync(statusId, ct);
        
        return role.Match<IActionResult>(
            r => Ok(r),
            () => NotFound(new { Message = $"Status '{id}' not found." })
        );
    }
    
    [HttpGet("Get-All-Statuses")]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var roles = await _questionnaireStatusQuery.GetAllAsync(ct);
        return Ok(roles);
    }
}