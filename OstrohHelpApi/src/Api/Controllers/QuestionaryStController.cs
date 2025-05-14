using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using Application.QuestionnaireStatus.Commands;
using Domain.Inventory.Statuses;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/QuestiStatController")]
public class QuestionaryStController(IMediator _mediator, 
    IQuestionnaireStatusQuery _questionnaireStatusQuery, 
    IQuestionnaireStatusRepository _questionnaireStatusRepository) : ControllerBase
{
    [HttpPost("Create-QuestionaryStatus")]
    public async Task<IActionResult> Create([FromBody] CreateQuestionaryStatusCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);

        return result.Match<IActionResult>(
            status => CreatedAtAction(nameof(Create), new { id = status.Id }, status),
            ex => BadRequest(new { Error = ex.Message })
        );
    }
    
    [HttpPut("Update-StatusQuestionary")]
    public async Task<IActionResult> Update([FromBody] UpdateQuestionaryStatusCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return result.Match<IActionResult>(
            _ => NoContent(),
            ex => BadRequest(new { Error = ex.Message })
        );
    }
    
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
    
    [HttpDelete("{id} Delete Status")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var command = new DeleteQuestionaryStatusCommand(id);
        var result = await _mediator.Send(command, ct);

        return result.Match(
            _ => (IActionResult)NoContent(),
            ex => BadRequest(new { Error = ex.Message })
        );
    }
    
    
    
}