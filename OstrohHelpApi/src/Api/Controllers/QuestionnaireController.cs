using Api.Dtos;
using Application.Common.Interfaces.Queries;
using Application.Questionnaire.Commands;
using AutoMapper;
using Domain.Inventory;
using Domain.Users;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;


[ApiController]
[Route("api/questionnaire")]
public class QuestionnaireController(
    IMediator _mediator, 
    IQuestionnaireQuery _questionnaireQuery,
    IMapper _mapper) : ControllerBase
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
        var questionnaireId = new QuestionaryId(id);
        //Один запит до БД з Include замість N+1
        var option = await _questionnaireQuery.GetByIdWithDetailsAsync(questionnaireId, ct);

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
        var idUser = new UserId(id);
        // Один запит до БД з Include замість N+1
        var questionnaires = await _questionnaireQuery.GetAllByUserIdWithDetailsAsync(idUser, ct);

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
        var userId = new UserId(id);
        // Один запит до БД з Include замість N+1
        var questionnaires = await _questionnaireQuery.GetAllByUserIdWithDetailsAsync(userId, ct);

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
    
    [HttpDelete("Delete-Questionnaire")]
    public async Task<IActionResult> Delete([FromBody]Guid id, CancellationToken ct)
    {
        var command = new DeleteQuestionnaireCommand(id);
        var result = await _mediator.Send(command, ct);

        return result.Match<IActionResult>(
            _ => NoContent(),
            ex => BadRequest(new { Error = ex.Message })
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
    
    [HttpPut("Update-StatusQuestionnaire")]
    public async Task<IActionResult> UpdateStatus([FromBody] UpdateStatusCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return result.Match<IActionResult>(
            _ => NoContent(),
            ex => BadRequest(new { Error = ex.Message })
        );
    }
}