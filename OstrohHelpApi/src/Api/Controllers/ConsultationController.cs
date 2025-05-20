using Api.Dtos;
using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using Application.Consultations.Commands;
using Application.Questionnaire.Commands;
using Domain.Conferences;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/Consultations")]
public class ConsultationController(IMediator _mediator, 
    IConsultationQuery _consultationQuery, 
    IConsultationRepository _consultationRepository,
    IConsultationStatusQuery _consultationStatusQuery,
    IUserQuery _userQuery,
    IQuestionnaireQuery _questionnaireQuery) : ControllerBase
{
    //Accept
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
    [HttpDelete("Delete-Consultation")]
    public async Task<IActionResult> Delete([FromBody] DeleteConsultationCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return result.Match<IActionResult>(
            _ => NoContent(),
            ex => BadRequest(new { Error = ex.Message })
        );
    }
    
    //GetAll
    //Достаємо консультацію потім по studentId та psychologistId виводимо ім'я(FullName) студента і психолога, а також статус консультації
    [HttpGet("Get-All-Consultations")]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var consultations = await _consultationQuery.GetAllAsync(ct);

        if (!consultations.Any())
            return NotFound(new { Message = "No consultations found." });

        var dtos = new List<ConsultationDto>();

        foreach (var c in consultations)
        {
            string studentName = "Анонімно";
            string psychologistName = "Невідомий";
            string statusName = "Невідомий";

            // --- Отримай статус ---
            var statusOption = await _consultationStatusQuery.GetByIdAsync(c.StatusId, ct);
            await statusOption.Match(
                s => Task.FromResult(statusName = s.Name),
                () => Task.CompletedTask
            );

            // --- Отримай студента ---
            var studentOption = await _userQuery.GetByIdAsync(c.StudentId, ct);
            await studentOption.Match(
                u => Task.FromResult(studentName = u.FullName),
                () => Task.CompletedTask
            );

            // --- Отримай психолога ---
            var psychologistOption = await _userQuery.GetByIdAsync(c.PsychologistId, ct);
            await psychologistOption.Match(
                p => Task.FromResult(psychologistName = p.FullName),
                () => Task.CompletedTask
            );

            dtos.Add(new ConsultationDto
            {
                Id = c.Id,
                QuestionnaireId = c.QuestionnaireId,
                StudentId = c.StudentId,
                StudentName = studentName,
                PsychologistId = c.PsychologistId,
                PsychologistName = psychologistName,
                StatusId = c.StatusId,
                StatusName = statusName,
                ScheduledTime = c.ScheduledTime,
                CreatedAt = c.CreatedAt
            });
        }

        return Ok(dtos);
    }
        
    
    //Теж саме і для GetById
    //GetById
    [HttpGet("Get-Consultation-ById")]
    public async Task<IActionResult> GetById([FromQuery] Guid consultationId, CancellationToken ct)
    {
        var id = new ConsultationsId(consultationId);
        var consultationOption = await _consultationQuery.GetByIdAsync(id, ct);

        return await consultationOption.Match<Task<IActionResult>>(
            async c =>
            {
                string studentName = "Анонімно";
                string psychologistName = "Невідомий";
                string statusName = "Невідомий";

                // --- Отримай статус ---
                var statusOption = await _consultationStatusQuery.GetByIdAsync(c.StatusId, ct);
                await statusOption.Match(
                    s => Task.FromResult(statusName = s.Name),
                    () => Task.CompletedTask
                );

                // --- Отримай студента ---
                var studentOption = await _userQuery.GetByIdAsync(c.StudentId, ct);
                await studentOption.Match(
                    u => Task.FromResult(studentName = u.FullName),
                    () => Task.CompletedTask
                );
                

                // --- Отримай психолога ---
                var psychologistOption = await _userQuery.GetByIdAsync(c.PsychologistId, ct);
                await psychologistOption.Match(
                    p => Task.FromResult(psychologistName = p.FullName),
                    () => Task.CompletedTask
                );
                

                // --- DTO ---
                var dto = new ConsultationDto
                {
                    Id = c.Id,
                    QuestionnaireId = c.QuestionnaireId,
                    StudentId = c.StudentId,
                    StudentName = studentName,
                    PsychologistId = c.PsychologistId,
                    PsychologistName = psychologistName,
                    StatusId = c.StatusId,
                    StatusName = statusName,
                    ScheduledTime = c.ScheduledTime,
                    CreatedAt = c.CreatedAt
                };

                return Ok(dto);
            },
            () => Task.FromResult<IActionResult>(NotFound(new { Message = "Questionary not found" }))
        );
    }
    
    
    
    
}