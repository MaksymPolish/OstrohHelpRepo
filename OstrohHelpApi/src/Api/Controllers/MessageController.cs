using System.Runtime.InteropServices.JavaScript;
using Api.Dtos;
using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using Application.Messages.Commands;
using AutoMapper;
using Domain.Conferences;
using Domain.Messages;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/Message")]
public class MessageController(IMediator _mediator, 
    IMessageQuery _messageQuery, 
    IMapper _mapper,
    IUserQuery _userQuery) : ControllerBase
{
    //Recive by Consultation
    [HttpGet("Recive")]
    public async Task<IActionResult> Recive([FromQuery] Guid idConsultation, CancellationToken ct)
    {
        var consultationId = new ConsultationsId(idConsultation);
        var messagesOption = await _messageQuery.GetAllMessagesByConsultationId(consultationId, ct);

        return await messagesOption.Match<Task<IActionResult>>(
            async messages =>
            {
                var dtos = new List<MessageDto>();

                foreach (var message in messages)
                {
                    string senderName = "Невідомий";
                    string receiverName = "Невідомий";

                    // --- Отримай імена ---
                    var senderOption = await _userQuery.GetByIdAsync(message.SenderId, ct);
                    var receiverOption = await _userQuery.GetByIdAsync(message.ReceiverId, ct);

                    senderName = senderOption.Match(u => u.FullName, () => "Невідомий");
                    receiverName = receiverOption.Match(u => u.FullName, () => "Невідомий");

                    // --- Додай до списку ---
                    var dto = _mapper.Map<MessageDto>(message);
                    dto.FullNameSender = senderName;
                    dto.FullNameReceiver = receiverName;
                    dtos.Add(dto);
                }

                return Ok(dtos);
            },
            () => Task.FromResult<IActionResult>(
                NotFound(new { Message = $"No messages found for consultation ID '{consultationId}'." })
            )
        );
    }

    //Send
    [HttpPost("Send")]
    public async Task<IActionResult> Send([FromBody] SendMessageCommand command, CancellationToken ct)
    {
        // command.MediaPaths може бути null або списком шляхів до медіа
        var result = await _mediator.Send(command, ct);

        return result.Match<IActionResult>(
            message => CreatedAtAction(nameof(Send), new { id = message.Id }, message),
            errors => BadRequest(new { Error = errors.Message })
        );
    }

    //Delete
    [HttpDelete("Delete")]
    public async Task<IActionResult> Delete([FromBody] DeleteMessageCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);

        return result.Match<IActionResult>(
            message => CreatedAtAction(nameof(Delete), new { id = message.Id }, message),
            errors => BadRequest(new {Error = errors.Message})
        );
    }
    
    //Read
    [HttpPut("mark-as-read")]
    public async Task<IActionResult> Read([FromBody] MarkMessageAsReadCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);

        return result.Match<IActionResult>(
            message => CreatedAtAction(nameof(Read), new { id = message.Id }, message),
            errors => BadRequest(new {Error = errors.Message})
        );
    }
    
}