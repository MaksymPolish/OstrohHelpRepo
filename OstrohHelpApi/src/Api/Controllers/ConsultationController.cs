using Api.Dtos;
using Api.Hubs;
using Application.Common.Interfaces.Queries;
using Application.Consultations.Commands;
using AutoMapper;
using Domain.Conferences;
using Domain.Users;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace Api.Controllers;

[ApiController]
[Route("api/Consultations")]
[Authorize] // Вимагає автентифікацію для всіх ендпоінтів
public class ConsultationController(
    IMediator _mediator, 
    IConsultationQuery _consultationQuery,
    IUserQuery _userQuery,
    IMapper _mapper,
    IHubContext<ChatHub> _hubContext) : ControllerBase
{
    //Accept
    [Authorize(Policy = "RequirePsychologist")] // Тільки психологи та керівники
    [HttpPost("Accept-Questionnaire")]
    public async Task<IActionResult> Accept([FromBody] AcceptQuestionnaireRequest request, CancellationToken ct)
    {
        var command = new AcceptQuestionnaireCommand(request.QuestionaryId, request.PsychologistId, request.ScheduledTime);
        var result = await _mediator.Send(command, ct);
        return await result.Match<Task<IActionResult>>(
            async consultation =>
            {
                var dto = _mapper.Map<ConsultationDto>(consultation);

                var student = await _userQuery.GetByIdAsync(consultation.StudentId, ct);
                var psychologist = await _userQuery.GetByIdAsync(consultation.PsychologistId, ct);

                if (student.HasValue)
                {
                    dto.StudentName = student.ValueOr((Domain.Users.User)null)?.FullName ?? "Невідомий";
                    dto.StudentPhotoUrl = student.ValueOr((Domain.Users.User)null)?.PhotoUrl;
                }
                if (psychologist.HasValue)
                {
                    dto.PsychologistName = psychologist.ValueOr((Domain.Users.User)null)?.FullName ?? "Невідомий";
                    dto.PsychologistPhotoUrl = psychologist.ValueOr((Domain.Users.User)null)?.PhotoUrl;
                }

                await NotifyConsultationStarted(consultation.Id.Value, dto, ct);

                return CreatedAtAction(nameof(GetById), new { id = consultation.Id.Value }, dto);
            },
            ex => Task.FromResult<IActionResult>(BadRequest(new { Error = ex.Message }))
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
            dto.StudentPhotoUrl = c.Student?.PhotoUrl;
            dto.PsychologistName = c.Psychologist?.FullName ?? "Невідомий";
            dto.PsychologistPhotoUrl = c.Psychologist?.PhotoUrl;
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
                dto.StudentPhotoUrl = c.Student?.PhotoUrl;
                dto.PsychologistName = c.Psychologist?.FullName ?? "Невідомий";
                dto.PsychologistPhotoUrl = c.Psychologist?.PhotoUrl;
                
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
            dto.StudentPhotoUrl = c.Student?.PhotoUrl;
            dto.PsychologistName = c.Psychologist?.FullName ?? "Невідомий";
            dto.PsychologistPhotoUrl = c.Psychologist?.PhotoUrl;
            return dto;
        }).ToList();

        return Ok(dtos);
    }

    private async Task NotifyConsultationStarted(Guid consultationId, ConsultationDto consultationInfo, CancellationToken ct)
    {
        try
        {
            var notificationData = new
            {
                ConsultationId = consultationId.ToString(),
                StudentId = consultationInfo.StudentId,
                PsychologistId = consultationInfo.PsychologistId,
                StudentName = consultationInfo.StudentName,
                StudentPhotoUrl = consultationInfo.StudentPhotoUrl,
                PsychologistName = consultationInfo.PsychologistName,
                PsychologistPhotoUrl = consultationInfo.PsychologistPhotoUrl,
                ScheduledTime = consultationInfo.ScheduledTime,
                Message = $"Консультація розпочалась! {consultationInfo.StudentName} та {consultationInfo.PsychologistName}",
                Timestamp = DateTime.UtcNow
            };

            await _hubContext.Clients.User(consultationInfo.StudentId)
                .SendAsync("ConsultationStarted", notificationData, ct);

            await _hubContext.Clients.User(consultationInfo.PsychologistId)
                .SendAsync("ConsultationStarted", notificationData, ct);
        }
        catch
        {
            // Ignore notification failures. Consultation is already created.
        }
    }
    
    
}