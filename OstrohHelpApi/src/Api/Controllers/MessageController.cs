using System.Runtime.InteropServices.JavaScript;
using System.Security.Claims;
using Api.Dtos;
using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using Application.Common.Services;
using Application.Messages.Commands;
using AutoMapper;
using Domain.Conferences;
using Domain.Messages;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/Message")]
[Authorize] // Вимагає автентифікацію для всіх ендпоінтів
public class MessageController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IMessageQuery _messageQuery;
    private readonly IMapper _mapper;
    private readonly IUserQuery _userQuery;
    private readonly IConsultationAccessChecker _accessChecker;
    private readonly Application.Common.Interfaces.Services.IPreviewGenerationService _previewGenerationService;
    private readonly IAuditLogService _auditLogService;
    private readonly ILogger<MessageController> _logger;

    public MessageController(
        IMediator mediator,
        IMessageQuery messageQuery,
        IMapper mapper,
        IUserQuery userQuery,
        IConsultationAccessChecker accessChecker,
        Application.Common.Interfaces.Services.IPreviewGenerationService previewGenerationService,
        IAuditLogService auditLogService,
        ILogger<MessageController> logger)
    {
        _mediator = mediator;
        _messageQuery = messageQuery;
        _mapper = mapper;
        _userQuery = userQuery;
        _accessChecker = accessChecker;
        _previewGenerationService = previewGenerationService;
        _auditLogService = auditLogService;
        _logger = logger;
    }

    /// <summary>
    /// Batch upload multiple files and create attachments in one request.
    /// Files are uploaded to Cloudinary in the "attachments" folder.
    /// Preview URLs are generated for each file based on its type.
    /// Supports single or multiple file uploads.
    /// </summary>
    /// <remarks>
    /// Usage:
    /// - Without message attachment: POST /api/Message/BatchUpload
    /// - With message attachment: POST /api/Message/BatchUpload?messageId={guid}
    /// </remarks>
    [HttpPost("BatchUpload")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(AddMultipleAttachmentsResponse), 200)]
    [ProducesResponseType(typeof(object), 400)]
    [ProducesResponseType(typeof(object), 401)]
    [ProducesResponseType(typeof(object), 500)]
    public async Task<IActionResult> BatchUpload(IFormFileCollection files, [FromQuery] Guid? messageId, CancellationToken ct)
    {
        var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (currentUserId == null)
            return Unauthorized("User identity not found");

        if (files == null || files.Count == 0)
            return BadRequest("No files provided");

        // Validate messageId if provided
        if (messageId.HasValue && messageId.Value == Guid.Empty)
            return BadRequest("Invalid messageId");

        // Security check: if messageId is provided, verify ownership
        if (messageId.HasValue)
        {
            var isOwner = await _accessChecker.IsMessageOwner(Guid.Parse(currentUserId), messageId.Value, ct);
            if (!isOwner)
            {
                return Forbid("You can only add attachments to your own messages");
            }
        }

        // Convert IFormFileCollection to List<BatchFileUpload>
        var batchFiles = new List<Application.Messages.Commands.BatchFileUpload>();
        
        try
        {
            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    var fileStream = new MemoryStream();
                    await file.CopyToAsync(fileStream, ct);
                    fileStream.Position = 0;

                    batchFiles.Add(new Application.Messages.Commands.BatchFileUpload
                    {
                        FileStream = fileStream,
                        FileName = file.FileName,
                        FileType = Path.GetExtension(file.FileName).TrimStart('.'),
                        FileSizeBytes = file.Length
                    });
                }
            }

            if (batchFiles.Count == 0)
                return BadRequest("No valid files provided");

            // Execute batch upload command
            var command = new AddMultipleAttachmentsCommand
            {
                Files = batchFiles,
                MessageId = messageId
            };

            var result = await _mediator.Send(command, ct);
            
            // Log audit for successful attachment upload
            var ipAddress = HttpContext?.Connection.RemoteIpAddress?.ToString();
            var attachmentIds = (result as IEnumerable<object>)?.Select(x => x.GetType().GetProperty("Id")?.GetValue(x)) ?? Enumerable.Empty<object>();
            var details = System.Text.Json.JsonSerializer.Serialize(new
            {
                MessageId = messageId,
                AttachmentCount = batchFiles.Count,
                TotalSizeBytes = batchFiles.Sum(f => f.FileSizeBytes)
            });
            
            await _auditLogService.LogAsync(
                Guid.Parse(currentUserId),
                "UploadAttachment",
                "Attachment",
                messageId ?? Guid.Empty,
                ipAddress,
                details
            );
            
            return Ok(result);
        }
        finally
        {
            // Clean up streams - always execute even if exception occurs
            foreach (var file in batchFiles)
            {
                file.FileStream?.Dispose();
            }
        }
    }
    
    [HttpGet("Recive")]
    public async Task<IActionResult> Recive([FromQuery] Guid idConsultation, CancellationToken ct)
    {
        var consultationId = new ConsultationsId(idConsultation);
        var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        // SECURITY: Перевірити, чи користувач є членом цієї консультації
        if (currentUserId == null)
            return Unauthorized("User identity not found");
        
        var isMember = await _accessChecker.IsConsultationMember(
            Guid.Parse(currentUserId), idConsultation, ct);
        
        if (!isMember)
        {
            return Forbid("You are not a member of this consultation");
        }

        var messagesOption = await _messageQuery.GetAllMessagesByConsultationId(consultationId, ct);

        return await messagesOption.Match<Task<IActionResult>>(
            async messages =>
            {
                var dtos = new List<MessageDto>();

                // TODO: OPTIMIZATION - N+1 Query Issue
                // Currently: For N messages, performs 2*N database queries (one per sender/receiver)
                // IMPROVEMENT: Batch load all unique users in a single query
                // Collect all unique user IDs first, then load them in one batch query
                // var allUserIds = messages
                //     .SelectMany(m => new[] { m.SenderId, m.ReceiverId })
                //     .Distinct()
                //     .ToList();
                // var users = await _userQuery.GetByIdsAsync(allUserIds, ct);
                // Then use dictionary lookup instead of individual queries

                foreach (var message in messages)
                {
                    string senderName = "Невідомий";
                    string receiverName = "Невідомий";

                    // --- Отримай імена (N+1 Query - see TODO above) ---
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
        var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var result = await _mediator.Send(command, ct);

        if (result.IsSuccess)
        {
            // Log audit for successful message deletion
            var ipAddress = HttpContext?.Connection.RemoteIpAddress?.ToString();
            var details = System.Text.Json.JsonSerializer.Serialize(new
            {
                MessageId = command.MessageId
            });
            
            await _auditLogService.LogAsync(
                Guid.Parse(currentUserId),
                "DeleteMessage",
                "Message",
                command.MessageId,
                ipAddress,
                details
            );
        }

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

    /// <summary>
    /// Soft delete an attachment - clears its data and marks as deleted.
    /// File remains on Cloudinary but is hidden from users.
    /// </summary>
    [HttpDelete("Attachment/{attachmentId}")]
    [ProducesResponseType(typeof(MessageAttachmentDto), 200)]
    [ProducesResponseType(typeof(object), 404)]
    [ProducesResponseType(typeof(object), 500)]
    public async Task<IActionResult> DeleteAttachment(Guid attachmentId, CancellationToken ct)
    {
        var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var command = new DeleteAttachmentCommand(attachmentId);
        var result = await _mediator.Send(command, ct);

        if (result.IsSuccess)
        {
            // Log audit for successful attachment deletion
            var ipAddress = HttpContext?.Connection.RemoteIpAddress?.ToString();
            var details = System.Text.Json.JsonSerializer.Serialize(new
            {
                AttachmentId = attachmentId
            });
            
            await _auditLogService.LogAsync(
                Guid.Parse(currentUserId),
                "DeleteAttachment",
                "Attachment",
                attachmentId,
                ipAddress,
                details
            );
        }

        return result.Match<IActionResult>(
            attachment =>
            {
                var dto = _mapper.Map<MessageAttachmentDto>(attachment);
                return Ok(new { message = "Attachment deleted", data = dto });
            },
            error => NotFound(new { error })
        );
    }
    
}