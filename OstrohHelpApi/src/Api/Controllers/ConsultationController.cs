using Api.Dtos;
using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
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
public class ConsultationController(IMediator _mediator, 
    IConsultationQuery _consultationQuery, 
    IConsultationRepository _consultationRepository,
    IConsultationStatusQuery _consultationStatusQuery,
    IUserQuery _userQuery,
    IConsultationStatusQuery _statusQuery,
    IMapper _mapper,
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
    
    [HttpGet("all")]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var consultations = await _consultationQuery.GetAllAsync(ct);

        var dtos = new List<ConsultationDto>();

        foreach (var consultation in consultations)
        {
            var statusOption = await _statusQuery.GetByIdAsync(consultation.StatusId, ct);
            var psychologistOption = await _userQuery.GetByIdAsync(consultation.PsychologistId, ct);
            var studentOption = await _userQuery.GetByIdAsync(consultation.StudentId, ct);

            var statusName = statusOption.Map(s => s.Name).ValueOr("Невідомий");
            var studentName = studentOption.Map(u => u.FullName).ValueOr("Невідомий");
            var psychologistName = psychologistOption.Map(u => u.FullName).ValueOr("Невідомий");
            
            var dto = _mapper.Map<ConsultationDto>(consultation);
            dto.StatusName = statusName;
            dto.StudentName = studentName;
            dto.PsychologistName = psychologistName;

            dtos.Add(dto);
        }

        return Ok(dtos);
    }
        
    
    //Теж саме і для GetById
    //GetById
    [HttpGet("Get-Consultation-ById/{id}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var consultationId = new ConsultationsId(id);
        var option = await _consultationQuery.GetByIdAsync(consultationId, ct);

        return await option.Match<Task<IActionResult>>(
            async c =>
            {
                var statusOption = await _statusQuery.GetByIdAsync(c.StatusId, ct);
                var studentOption = await _userQuery.GetByIdAsync(c.StudentId, ct);
                var psychologistOption = await _userQuery.GetByIdAsync(c.PsychologistId, ct);

                // --- Мапінг до DTO ---
                var dto = _mapper.Map<ConsultationDto>(c);
                dto.StatusName = statusOption.Map(s => s.Name).ValueOr("Невідомий");
                dto.StudentName = studentOption.Map(u => u.FullName).ValueOr("Невідомий");
                dto.PsychologistId = psychologistOption.Map(u => u.FullName).ValueOr("Невідомий");
                
                return Ok(dto);
            },
            () => Task.FromResult<IActionResult>(
                NotFound(new { Message = $"Consultation with ID '{id}' not found." })
            )
        );
    }
    
    [HttpGet("Get-All-Consultations-By-UserId/{Id}")]
    public async Task<IActionResult> GetAllByUserId(Guid Id, CancellationToken ct)
    {
        var userId = new UserId(Id);
        
        var consultations = await _consultationQuery.GetAllByUserIdAsync(userId, ct);

        foreach (var consultation  in consultations)
        {
            var statusOption = await _consultationStatusQuery.GetByIdAsync(consultation.StatusId, ct);
            var psychologistOption = await _userQuery.GetByIdAsync(consultation.PsychologistId, ct);
            var studentOption = await _userQuery.GetByIdAsync(consultation.StudentId, ct);

            var statusName = statusOption.Map(s => s.Name).ValueOr("Невідомий");
            var studentName = studentOption.Map(u => u.FullName).ValueOr("Невідомий");
            var psychologistName = psychologistOption.Map(u => u.FullName).ValueOr("Невідомий");
            
            var dto = _mapper.Map<ConsultationDto>(consultation);
            dto.StatusName = statusName;
            dto.StudentName = studentName;
            dto.PsychologistName = psychologistName;    

            return Ok(dto);
        }
        
        return Ok(consultations);
    }
    
    
}