using Api.Dtos;
using Application.Common.Interfaces.Queries;
using Application.Questionnaire.Commands;
using AutoMapper;
using Domain.Inventory;
using Domain.Users;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Api.Controllers;


[ApiController]
[Route("api/questionnaire")]
[Authorize] // Вимагає автентифікацію для всіх ендпоінтів
public class QuestionnaireController(
    IMediator _mediator, 
    IQuestionnaireQuery _questionnaireQuery,
    IMapper _mapper,
    ILogger<QuestionnaireController> _logger) : ControllerBase
{
    [HttpPost("Create-Questionnaire")]
    public async Task<IActionResult> Create([FromBody] CreateQuestionnaireCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);

        return result.Match<IActionResult>(
            q => CreatedAtAction(nameof(Create), new { id = q.Id }, q),
            ex => BadRequest(new { Error = ex.Message })
        );
    }
    
    [HttpGet("all")]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        // Один запит до БД з Include замість N+1
        var questionnaires = await _questionnaireQuery.GetAllWithDetailsAsync(ct);

        var dtos = questionnaires.Select(q =>
        {
            var dto = _mapper.Map<QuestionnaireDto>(q);
            dto.FullName = q.User?.FullName ?? "Невідомий";
            dto.Email = q.User?.Email ?? "Невідомий";
            dto.StatusName = q.Status?.Name ?? "Невідомий";
            return dto;
        }).ToList();

        return Ok(dtos);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        //Один запит до БД з Include замість N+1
        var option = await _questionnaireQuery.GetByIdWithDetailsAsync(id, ct);

        return option.Match<IActionResult>(
            q =>
            {
                var dto = _mapper.Map<QuestionnaireDto>(q);
                dto.FullName = q.User?.FullName ?? "Невідомий";
                dto.Email = q.User?.Email ?? "Невідомий";
                dto.StatusName = q.Status?.Name ?? "Невідомий";

                return Ok(dto);
            },
            () => NotFound(new { Message = $"Questionnaire with ID '{id}' not found." })
        );
    }
    
    [HttpGet("get-by-user-id/{id}")]
    public async Task<IActionResult> GetByUserId(Guid id, CancellationToken ct)
    {
        // Один запит до БД з Include замість N+1
        var questionnaires = await _questionnaireQuery.GetAllByUserIdWithDetailsAsync(id, ct);

        var dtos = questionnaires.Select(q =>
        {
            var dto = _mapper.Map<QuestionnaireDto>(q);
            dto.FullName = q.User?.FullName ?? "Невідомий";
            dto.Email = q.User?.Email ?? "Невідомий";
            dto.StatusName = q.Status?.Name ?? "Невідомий";
            return dto;
        }).ToList();

        return Ok(dtos);
    }
    
    [HttpGet("Get-All-Questionnaire-By-UserId/{id}")]
    public async Task<IActionResult> GetAllByUserId(Guid id, CancellationToken ct)
    {
        // Один запит до БД з Include замість N+1
        var questionnaires = await _questionnaireQuery.GetAllByUserIdWithDetailsAsync(id, ct);

        var dtos = questionnaires.Select(q =>
        {
            var dto = _mapper.Map<QuestionnaireDto>(q);
            dto.FullName = q.User?.FullName ?? "Невідомий";
            dto.Email = q.User?.Email ?? "Невідомий";
            dto.StatusName = q.Status?.Name ?? "Невідомий";
            return dto;
        }).ToList();

        return Ok(dtos);
    }
    
    [Authorize(Policy = "RequireHeadOfService")] // Тільки керівник служби
    [HttpDelete("Delete-Questionnaire")]
    public async Task<IActionResult> Delete([FromBody] Guid id, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("ModelState validation failed in Delete method");
            return BadRequest(ModelState);
        }

        _logger.LogInformation("Delete questionnaire request for ID: {Id}", id);
        
        var command = new DeleteQuestionnaireCommand(id);
        var result = await _mediator.Send(command, ct);

        return result.Match<IActionResult>(
            _ => 
            {
                _logger.LogInformation("Questionnaire {Id} deleted successfully", id);
                return NoContent();
            },
            ex => 
            {
                _logger.LogError(ex, "Failed to delete questionnaire {Id}", id);
                return BadRequest(new { Error = ex.Message });
            }
        );
    }
    
    [HttpPut("Update-Questionnaire")]
    public async Task<IActionResult> Update([FromBody] UpdateQuestionnaireCommand command, CancellationToken ct)
    {
        // Send command to mediator
        var result = await _mediator.Send(command, ct);

        // Return Ok if successful, BadRequest otherwise
        return result.Match<IActionResult>(
            _ => NoContent(), // successful
            ex => BadRequest(new { Error = ex.Message }) // failed
        );
    }
    
    [Authorize(Policy = "RequirePsychologist")] // Тільки психологи та керівники
    [HttpPut("Update-StatusQuestionnaire")]
    [HttpPut("UpdateStatus")] // Alternative route for frontend
    [HttpPut("Update-Status")] // Alternative route for frontend
    public async Task<IActionResult> UpdateStatus([FromBody] UpdateStatusCommand command, CancellationToken ct)
    {
        _logger.LogInformation("Received UpdateStatus request for QuestionnaireId: {Id} with StatusId: {StatusId}", 
            command.Id, command.StatusId);
        // Логування валідації ModelState
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors);
            _logger.LogWarning("ModelState validation failed. Errors: {Errors}", 
                string.Join("; ", errors.Select(e => e.ErrorMessage)));
            
            return BadRequest(new 
            { 
                Error = "Validation failed", 
                Details = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => new { Message = e.ErrorMessage })
                    .ToList()
            });
        }

        // Логування запиту
        _logger.LogInformation("UpdateStatus request: QuestionnaireId={Id}, StatusId={StatusId}", 
            command.Id, command.StatusId);

        try
        {
            var result = await _mediator.Send(command, ct);
            
            return result.Match<IActionResult>(
                q => 
                {
                    _logger.LogInformation("Questionnaire {Id} status updated to {StatusId}", 
                        command.Id, command.StatusId);
                    return NoContent();
                },
                ex => 
                {
                    _logger.LogError(ex, "Failed to update questionnaire {Id} status", command.Id);
                    return BadRequest(new { Error = ex.Message });
                }
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in UpdateStatus: {Message}", ex.Message);
            return BadRequest(new { Error = ex.Message, Details = ex.InnerException?.Message });
        }
    }

}