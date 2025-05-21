using System.Runtime.InteropServices.JavaScript;
using Api.Dtos;
using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using Application.Messages.Commands;
using Domain.Conferences;
using Domain.Messages;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/Message")]
public class MessageController(IMediator _mediator, 
    IMessageQuery _messageQuery, 
    IMessageRepository _messageRepository,
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

                    // --- Отримай відправника ---
                    var senderOption = await _userQuery.GetByIdAsync(message.SenderId, ct);
                    senderOption.Match(
                        user => { senderName = user.FullName; },
                        () => { }
                    );

                    // --- Отримай отримувача ---
                    var receiverOption = await _userQuery.GetByIdAsync(message.ReceiverId, ct);
                    receiverOption.Match(
                        user => { receiverName = user.FullName; },
                        () => { }
                    );

                    // --- Додай DTO ---
                    dtos.Add(new MessageDto(
                        Id: message.Id.ToString(),
                        ConsultationId: message.ConsultationId.ToString(),
                        SenderId: message.SenderId.ToString(),
                        ReceiverId: message.ReceiverId.ToString(),
                        Text: message.Text,
                        IsRead: message.IsRead,
                        SentAt: message.SentAt,
                        FullNameSender: senderName,
                        FullNameReceiver: receiverName));
                }

                return Ok(dtos);
            },
            () => Task.FromResult<IActionResult>(NotFound(new { Message = $"No messages found for consultation ID '{consultationId}'." }))
        );
    }
    
    //Send
    [HttpPost("Send")]
    public async Task<IActionResult> Send([FromBody] SendMessageCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);

        return result.Match<IActionResult>(
            message => CreatedAtAction(nameof(Send), new { id = message.Id }, message),
            errors => BadRequest(new {Error = errors.Message})
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
    //Update
    
    
}