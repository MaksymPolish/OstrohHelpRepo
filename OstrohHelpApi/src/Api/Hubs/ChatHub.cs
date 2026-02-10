using Api.Dtos;
using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using Application.Messages.Commands;
using AutoMapper;
using Domain.Conferences;
using Domain.Messages;
using Domain.Users;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Api.Hubs;

/// <summary>
/// SignalR Hub для real-time чату
/// </summary>
[Authorize]
public class ChatHub : Hub
{
    private readonly IMediator _mediator;
    private readonly IMessageQuery _messageQuery;
    private readonly IMapper _mapper;
    private readonly IUserQuery _userQuery;
    private readonly ILogger<ChatHub> _logger;
    private readonly IMessageAttachmentRepository _attachmentRepository;

    public ChatHub(
        IMediator mediator,
        IMessageQuery messageQuery,
        IMapper mapper,
        IUserQuery userQuery,
        ILogger<ChatHub> logger,
        IMessageAttachmentRepository attachmentRepository)
    {
        _mediator = mediator;
        _messageQuery = messageQuery;
        _mapper = mapper;
        _userQuery = userQuery;
        _logger = logger;
        _attachmentRepository = attachmentRepository;
    }

    /// <summary>
    /// Підключення до консультації (приєднання до групи)
    /// </summary>
    public async Task JoinConsultation(string consultationId)
    {
        var userId = Context.User?.FindFirst("sub")?.Value 
                     ?? Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        await Groups.AddToGroupAsync(Context.ConnectionId, consultationId);
        
        _logger.LogInformation(
            "User {UserId} joined consultation {ConsultationId} with connection {ConnectionId}",
            userId, consultationId, Context.ConnectionId);

        // Повідомити інших учасників
        await Clients.OthersInGroup(consultationId).SendAsync("UserJoined", new
        {
            UserId = userId,
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Вийти з консультації
    /// </summary>
    public async Task LeaveConsultation(string consultationId)
    {
        var userId = Context.User?.FindFirst("sub")?.Value 
                     ?? Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, consultationId);
        
        _logger.LogInformation(
            "User {UserId} left consultation {ConsultationId}",
            userId, consultationId);

        await Clients.OthersInGroup(consultationId).SendAsync("UserLeft", new
        {
            UserId = userId,
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Відправити повідомлення в консультацію
    /// </summary>
    public async Task SendMessage(string consultationId, string text, 
        List<AttachmentData>? attachments = null)
    {
        try
        {
            var senderId = Context.User?.FindFirst("sub")?.Value 
                          ?? Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(senderId))
            {
                await Clients.Caller.SendAsync("Error", "User not authenticated");
                return;
            }

            // Створити command для відправки повідомлення
            // receiverId визначається автоматично в команді на основі consultation
            var command = new SendMessageCommand(
                ConsultationId: Guid.Parse(consultationId),
                SenderId: Guid.Parse(senderId),
                Text: text,
                MediaPaths: null  // Attachments обробляються нижче
            );

            var result = await _mediator.Send(command);

            await result.Match(
                async message =>
                {
                    // Додати attachments якщо є
                    if (attachments != null && attachments.Any())
                    {
                        foreach (var attachment in attachments)
                        {
                            var messageAttachment = new MessageAttachment
                            {
                                Id = Guid.NewGuid(),
                                MessageId = message.Id,
                                FileUrl = attachment.FileUrl,
                                FileType = attachment.FileType,
                                CreatedAt = DateTime.UtcNow
                            };
                            await _attachmentRepository.AddAsync(messageAttachment, CancellationToken.None);
                        }
                    }

                    // Отримати повне повідомлення з attachments
                    var fullMessageOption = await _messageQuery.GetMessageById(message.Id, CancellationToken.None);
                    
                    await fullMessageOption.Match(
                        async fullMessage =>
                        {
                            // Отримати імена користувачів
                            var senderOption = await _userQuery.GetByIdAsync(fullMessage.SenderId, CancellationToken.None);
                            var receiverOption = await _userQuery.GetByIdAsync(fullMessage.ReceiverId, CancellationToken.None);

                            var dto = _mapper.Map<MessageDto>(fullMessage);
                            dto.FullNameSender = senderOption.Match(u => u.FullName, () => "Невідомий");
                            dto.FullNameReceiver = receiverOption.Match(u => u.FullName, () => "Невідомий");

                            // Відправити всім в групі консультації
                            await Clients.Group(consultationId).SendAsync("ReceiveMessage", dto);
                            
                            _logger.LogInformation(
                                "Message {MessageId} sent to consultation {ConsultationId}",
                                message.Id, consultationId);
                        },
                        async () =>
                        {
                            _logger.LogWarning("Message created but not found: {MessageId}", message.Id);
                            await Clients.Caller.SendAsync("Error", "Message created but could not be retrieved");
                        }
                    );
                },
                async error =>
                {
                    _logger.LogError("Failed to send message: {Error}", error.Message);
                    await Clients.Caller.SendAsync("Error", error.Message);
                }
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in SendMessage");
            await Clients.Caller.SendAsync("Error", "Failed to send message");
        }
    }

    /// <summary>
    /// Позначити повідомлення як прочитане
    /// </summary>
    public async Task MarkAsRead(string messageId, string consultationId)
    {
        try
        {
            var command = new MarkMessageAsReadCommand(MessageId: Guid.Parse(messageId));
            await _mediator.Send(command);

            // Повідомити інших користувачів в консультації
            await Clients.OthersInGroup(consultationId).SendAsync("MessageRead", new
            {
                MessageId = messageId,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking message as read");
            await Clients.Caller.SendAsync("Error", "Failed to mark message as read");
        }
    }

    /// <summary>
    /// Typing indicator - користувач друкує
    /// </summary>
    public async Task Typing(string consultationId)
    {
        var userId = Context.User?.FindFirst("sub")?.Value 
                     ?? Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        await Clients.OthersInGroup(consultationId).SendAsync("UserTyping", new
        {
            UserId = userId,
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Stop typing indicator
    /// </summary>
    public async Task StopTyping(string consultationId)
    {
        var userId = Context.User?.FindFirst("sub")?.Value 
                     ?? Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        await Clients.OthersInGroup(consultationId).SendAsync("UserStoppedTyping", new
        {
            UserId = userId
        });
    }

    /// <summary>
    /// Видалити повідомлення
    /// </summary>
    public async Task DeleteMessage(string messageId, string consultationId)
    {
        try
        {
            var command = new DeleteMessageCommand(MessageId: Guid.Parse(messageId));
            var result = await _mediator.Send(command);

            await result.Match(
                async _ =>
                {
                    // Повідомити всіх в групі про видалення
                    await Clients.Group(consultationId).SendAsync("MessageDeleted", new
                    {
                        MessageId = messageId,
                        Timestamp = DateTime.UtcNow
                    });
                },
                async error =>
                {
                    await Clients.Caller.SendAsync("Error", error.Message);
                }
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting message");
            await Clients.Caller.SendAsync("Error", "Failed to delete message");
        }
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst("sub")?.Value 
                     ?? Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        _logger.LogInformation("User {UserId} connected with connection {ConnectionId}", 
            userId, Context.ConnectionId);
        
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirst("sub")?.Value 
                     ?? Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        _logger.LogInformation("User {UserId} disconnected. Connection {ConnectionId}", 
            userId, Context.ConnectionId);
        
        await base.OnDisconnectedAsync(exception);
    }
}

/// <summary>
/// Клас для передачі даних про attachment
/// </summary>
public class AttachmentData
{
    public string FileUrl { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
}
