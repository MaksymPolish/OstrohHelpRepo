using Api.Dtos;
using Api.Hubs;
using Application.Common.Interfaces.Queries;
using Application.Consultations.Commands;
using Application.Questionnaire.Commands;
using AutoMapper;
using Domain.Inventory;
using Domain.Users;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace Api.Controllers;


[ApiController]
[Route("api/questionnaire")]
[Authorize] // Вимагає автентифікацію для всіх ендпоінтів
public class QuestionnaireController(
    IMediator _mediator, 
    IQuestionnaireQuery _questionnaireQuery,
    IUserQuery _userQuery,
    IMapper _mapper,
    IHubContext<ChatHub> _hubContext) : ControllerBase
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
    
    [Authorize(Policy = "RequirePsychologist")] // Тільки психологи та керівники
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
    
    [Authorize(Policy = "RequireHeadOfService")] // Тільки керівник служби
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
    
    [Authorize(Policy = "RequirePsychologist")] // Тільки психологи та керівники
    [HttpPut("Update-StatusQuestionnaire")]
    public async Task<IActionResult> UpdateStatus([FromBody] UpdateStatusCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return result.Match<IActionResult>(
            _ => NoContent(),
            ex => BadRequest(new { Error = ex.Message })
        );
    }

    /// <summary>
    /// Прийняти анкету та створити консультацію для чату
    /// </summary>
    /// <remarks>
    /// Цей endpoint:
    /// 1. Прймає анкету студента
    /// 4. Відправляє SignalR notification обом користувачам
    /// 
    /// Приклад запиту:
    /// {
    ///   "QuestionaryId": "550e8400-e29b-41d4-a716-446655440000",
    ///   "PsychologistId": "660e8400-e29b-41d4-a716-446655440000",
    ///   "ScheduledTime": "2026-02-10T15:30:00Z"
    /// }
    /// </remarks>
    [Authorize(Policy = "RequirePsychologist")] // Тільки психологи та керівники
    [HttpPost("accept")]
    [ProducesResponseType(typeof(ConsultationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Accept([FromBody] AcceptQuestionnaireRequest request, CancellationToken ct)
    {
        var command = new AcceptQuestionnaireCommand(
            request.QuestionaryId,
            request.PsychologistId,
            request.ScheduledTime
        );

        var result = await _mediator.Send(command, ct);

        return await result.Match<Task<IActionResult>>(
            async consultation =>
            {
                var dto = _mapper.Map<ConsultationDto>(consultation);
                
                // Отримай інформацію про студента та психолога для поліпшеного DTO
                var studentTask = _userQuery.GetByIdAsync(consultation.StudentId, ct);
                var psychologistTask = _userQuery.GetByIdAsync(consultation.PsychologistId, ct);
                
                await Task.WhenAll(studentTask, psychologistTask);
                
                var student = await studentTask;
                var psychologist = await psychologistTask;
                
                if (student.HasValue)
                    dto.StudentName = student.ValueOr((Domain.Users.User)null)?.FullName ?? "Невідомий";
                if (psychologist.HasValue)
                    dto.PsychologistName = psychologist.ValueOr((Domain.Users.User)null)?.FullName ?? "Невідомий";

                // 📢 Відправити SignalR notification обом користувачам
                await NotifyConsultationStarted(consultation.Id.Value, dto, ct);

                return CreatedAtAction(nameof(Accept), new { id = consultation.Id }, dto);
            },
            ex => Task.FromResult<IActionResult>(BadRequest(new { Error = ex.Message }))
        );
    }

    /// <summary>
    /// Відправляє SignalR notification обом користувачам консультації
    /// </summary>
    private async Task NotifyConsultationStarted(Guid consultationId, ConsultationDto consultationInfo, CancellationToken ct)
    {
        try
        {
            // 📤 Відправити "ConsultationStarted" event обом користувачам
            var notificationData = new
            {
                ConsultationId = consultationId.ToString(),
                StudentId = consultationInfo.StudentId,
                PsychologistId = consultationInfo.PsychologistId,
                StudentName = consultationInfo.StudentName,
                PsychologistName = consultationInfo.PsychologistName,
                ScheduledTime = consultationInfo.ScheduledTime,
                Message = $"Консультація розпочалась! {consultationInfo.StudentName} та {consultationInfo.PsychologistName}",
                Timestamp = DateTime.UtcNow
            };

            // Відправити notification обом користувачам
            await _hubContext.Clients.User(consultationInfo.StudentId)
                .SendAsync("ConsultationStarted", notificationData, ct);
            
            await _hubContext.Clients.User(consultationInfo.PsychologistId)
                .SendAsync("ConsultationStarted", notificationData, ct);

            // Опціонально: залогувати це
            Console.WriteLine($"✅ Consultation {consultationId} started. Notifications sent to {consultationInfo.StudentId} and {consultationInfo.PsychologistId}");
        }
        catch (Exception ex)
        {
            // Якщо notification не відправилася, логуємо але не кидаємо exception
            // (консультація все одно створена)
            Console.WriteLine($"⚠️ Failed to send consultation started notification: {ex.Message}");
        }
    }
}