using Api.Dtos;
using Application.Common.Interfaces.Queries;
using Application.Consultations.Commands;
using Application.Questionnaire.Commands;
using AutoMapper;
using Domain.Conferences;
using Domain.Users;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/Consultations")]
[Authorize] // Вимагає автентифікацію для всіх ендпоінтів
public class ConsultationController(
    IMediator _mediator, 
    IConsultationQuery _consultationQuery,
    IMapper _mapper) : ControllerBase
{
    //Accept
    [Authorize(Policy = "RequirePsychologist")] // Тільки психологи та керівники
    [HttpPost("Accept-Questionnaire")]
    public async Task<IActionResult> Accept([FromBody] AcceptQuestionnaireCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return result.Match<IActionResult>(
            _ => NoContent(),
            ex => BadRequest(new { Error = ex.Message })
        );
    }
    
    //Update
    [Authorize(Policy = "RequirePsychologist")] // Тільки психологи та керівники
    [HttpPut("Update-Consultation")]
    public async Task<IActionResult> Update([FromBody] UpdateConsultationCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return result.Match<IActionResult>(
            _ => NoContent(),
            ex => BadRequest(new { Error = ex.Message })
        );
    }
    //Delete
    [Authorize(Policy = "RequireHeadOfService")] // Тільки керівник служби
    [HttpDelete("Delete-Consultation")]
    public async Task<IActionResult> Delete([FromBody] DeleteConsultationCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return result.Match<IActionResult>(
            _ => NoContent(),
            ex => BadRequest(new { Error = ex.Message })
        );
    }
    
    [Authorize(Policy = "RequirePsychologist")] // Тільки психологи та керівники
    [HttpGet("all")]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        // Один запит до БД з Include замість N+1
        var consultations = await _consultationQuery.GetAllWithDetailsAsync(ct);

        var dtos = consultations.Select(c =>
        {
            var dto = _mapper.Map<ConsultationDto>(c);
            dto.StatusName = c.Status?.Name ?? "Невідомий";
            dto.StudentName = c.Student?.FullName ?? "Невідомий";
            dto.PsychologistName = c.Psychologist?.FullName ?? "Невідомий";
            return dto;
        }).ToList();

        return Ok(dtos);
    }
        
    
    //GetById
    [HttpGet("Get-Consultation-ById/{id}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var consultationId = new ConsultationsId(id);
        // Один запит до БД з Include замість N+1
        var option = await _consultationQuery.GetByIdWithDetailsAsync(consultationId, ct);

        return option.Match<IActionResult>(
            c =>
            {
                var dto = _mapper.Map<ConsultationDto>(c);
                dto.StatusName = c.Status?.Name ?? "Невідомий";
                dto.StudentName = c.Student?.FullName ?? "Невідомий";
                dto.PsychologistName = c.Psychologist?.FullName ?? "Невідомий";
                
                return Ok(dto);
            },
            () => NotFound(new { Message = $"Consultation with ID '{id}' not found." })
        );
    }
    
    [HttpGet("Get-All-Consultations-By-UserId/{Id}")]
    public async Task<IActionResult> GetAllByUserId(Guid Id, CancellationToken ct)
    {
        var userId = new UserId(Id);
    
        //  Один запит до БД з Include замість N+1
        var consultations = await _consultationQuery.GetAllByUserIdWithDetailsAsync(userId, ct);

        if (!consultations.Any())
            return NotFound(new { Message = $"No consultations found for user ID '{userId}'." });

        var dtos = consultations.Select(c =>
        {
            var dto = _mapper.Map<ConsultationDto>(c);
            dto.StatusName = c.Status?.Name ?? "Невідомий";
            dto.StudentName = c.Student?.FullName ?? "Невідомий";
            dto.PsychologistName = c.Psychologist?.FullName ?? "Невідомий";
            return dto;
        }).ToList();

        return Ok(dtos);
    }
    
    
}