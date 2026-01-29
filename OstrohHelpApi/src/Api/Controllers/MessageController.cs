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
public class MessageController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IMessageQuery _messageQuery;
    private readonly IMapper _mapper;
    private readonly IUserQuery _userQuery;
    private readonly Api.Services.CloudinaryService _cloudinaryService;

    public MessageController(
        IMediator mediator,
        IMessageQuery messageQuery,
        IMapper mapper,
        IUserQuery userQuery,
        Api.Services.CloudinaryService cloudinaryService)
    {
        _mediator = mediator;
        _messageQuery = messageQuery;
        _mapper = mapper;
        _userQuery = userQuery;
        _cloudinaryService = cloudinaryService;
    }

    //Upload file to Cloudinary
    /// Uploads a file (image/video/any) to Cloudinary for a specific user.
    [HttpPost("UploadToCloud/{userId}")]
    [Consumes("multipart/form-data")]
    [ApiExplorerSettings(IgnoreApi = false)]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(typeof(object), 400)]
    [ProducesResponseType(typeof(object), 500)]
    public async Task<IActionResult> UploadToCloud([FromRoute] string userId, IFormFile file, CancellationToken ct)
    {
        if (file == null || file.Length == 0 || string.IsNullOrWhiteSpace(userId))
            return BadRequest("No file or userId provided");

        var folder = $"users/{userId}";
        string url;
        using (var stream = file.OpenReadStream())
        {
            url = await _cloudinaryService.UploadFileAsync(stream, file.FileName, folder, file.ContentType);
        }
        if (string.IsNullOrEmpty(url))
            return StatusCode(500, "Upload to cloud failed");

        return Ok(new { url, fileType = file.ContentType });
    }
    
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