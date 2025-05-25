using Api.Dtos;
using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using Application.Questionnaire.Commands;
using AutoMapper;
using Domain.Inventory;
using Domain.Users;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;


[ApiController]
[Route("api/questionnaire")]
public class QuestionnaireController(IMediator _mediator, 
    IQuestionnaireQuery _questionnaireQuery, 
    IUserQuery _userQuery,
    IQuestionnaireStatusQuery _statusQuery,
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
        var questionnaires = await _questionnaireQuery.GetAllAsync(ct);
        var dtos = new List<QuestionnaireDto>();

        foreach (var q in questionnaires)
        {
            string fullName = "Невідомий";
            string email = "Невідомий";
            string statusName = "Невідомий";

            // --- Отримай студента ---
            if (q.UserId is not null)
            {
                var userOption = await _userQuery.GetByIdAsync(q.UserId, ct);
                userOption.Match(u =>
                {
                    fullName = u.FullName;
                    email = u.Email;
                }, () => { });
            }

            // --- Отримай статус ---
            var statusOption = await _statusQuery.GetByIdAsync(q.StatusId, ct);
            statusName = statusOption.Map(s => s.Name).ValueOr("Невідомий");

            // --- Мапінг до DTO ---
            var dto = _mapper.Map<QuestionnaireDto>(q);
            dto.FullName = fullName;
            dto.Email = email;
            dto.StatusName = statusName;

            dtos.Add(dto);
        }

        return Ok(dtos);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var questionnaireId = new QuestionaryId(id);
        var option = await _questionnaireQuery.GetByIdAsync(questionnaireId, ct);

        return await option.Match<Task<IActionResult>>(
            async q =>
            {
                string fullName = "Невідомий";
                string email = "Невідомий";

                // --- Отримай дані студента ---
                if (q.UserId is not null)
                {
                    var userOption = await _userQuery.GetByIdAsync(q.UserId, ct);
                    userOption.Match(u =>
                    {
                        fullName = u.FullName;
                        email = u.Email;
                    }, () => { });
                }

                // --- Отримай статус ---
                var statusOption = await _statusQuery.GetByIdAsync(q.StatusId, ct);
                var statusName = statusOption.Map(s => s.Name).ValueOr("Невідомий");

                // --- Мапінг до DTO ---
                var dto = _mapper.Map<QuestionnaireDto>(q);
                dto.FullName = fullName;
                dto.Email = email;
                dto.StatusName = statusName;

                return Ok(dto);
            },
            () => Task.FromResult<IActionResult>(
                NotFound(new { Message = $"Questionnaire with ID '{id}' not found." })
            )
        );
    }
    
    [HttpGet("get-by-user-id/{id}")]
    public async Task<IActionResult> GetByUserId(Guid id, CancellationToken ct)
    {
        var idUser = new UserId(id);
        var questionnaires = await _questionnaireQuery.GetByUserIdAsync(idUser, ct);
        var dtos = new List<QuestionnaireDto>();

        foreach (var q in questionnaires)
        {
            string fullName = "Невідомий";
            string email = "Невідомий";
            string statusName = "Невідомий";

            // --- Отримай студента ---
            if (q.UserId is not null)
            {
                var userOption = await _userQuery.GetByIdAsync(q.UserId, ct);
                userOption.Match(u =>
                {
                    fullName = u.FullName;
                    email = u.Email;
                }, () => { });
            }
            
            // --- Отримай статус ---
            var statusOption = await _statusQuery.GetByIdAsync(q.StatusId, ct);
            statusName = statusOption.Map(s => s.Name).ValueOr("Невідомий");

            // --- Мапінг до DTO ---
            var dto = _mapper.Map<QuestionnaireDto>(q);
            dto.FullName = fullName;
            dto.Email = email;
            dto.StatusName = statusName;

            dtos.Add(dto);
        }

        return Ok(dtos);
       
    }
    
    [HttpGet("Get-All-Questionnaire-By-UserId/{id}")]
    public async Task<IActionResult> GetAllByUserId(Guid id, CancellationToken ct)
    {
        var UserId = new UserId(id);
        var questionnaires = await _questionnaireQuery.GetAllByUserIdAsync(UserId, ct);
        var dtos = new List<QuestionnaireDto>();

        foreach (var q in questionnaires)
        {
            string fullName = "Невідомий";
            string email = "Невідомий";
            string statusName = "Невідомий";

            // --- Отримай студента ---
            if (q.UserId is not null)
            {
                var userOption = await _userQuery.GetByIdAsync(q.UserId, ct);
                userOption.Match(u =>
                {
                    fullName = u.FullName;
                    email = u.Email;
                }, () => { });
            }
            
            // --- Отримай статус ---
            var statusOption = await _statusQuery.GetByIdAsync(q.StatusId, ct);
            statusName = statusOption.Map(s => s.Name).ValueOr("Невідомий");

            // --- Мапінг до DTO ---
            var dto = _mapper.Map<QuestionnaireDto>(q);
            dto.FullName = fullName;
            dto.Email = email;
            dto.StatusName = statusName;

            dtos.Add(dto);
        }

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