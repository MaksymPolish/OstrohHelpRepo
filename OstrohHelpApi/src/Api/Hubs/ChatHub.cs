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
/// SignalR Hub для real-time чату консультацій
/// 
/// Модель: 1-на-1 консультація (1 студент + 1 психолог)
/// Чат створюється автоматично коли психолог приймає анкету
/// 
/// Потоки даних:
/// - При загрузці екрану чату -> JoinConsultation(consultationId)
/// - При закриvassureEguюч екрану -> LeaveConsultation(consultationId)
/// - При відправці -> SendMessage(consultationId, text, attachments)
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
    /// Користувач відкрив чат консультації
    /// Автоматично приєднується до групи консультації
    /// Повідомляє другого користувача що він онлайн
    /// </summary>
    public async Task JoinConsultation(string consultationId)
    {
        var userId = GetUserId();
        
        await Groups.AddToGroupAsync(Context.ConnectionId, consultationId);
        
        _logger.LogInformation(
            "User {UserId} opened consultation {ConsultationId}",
            userId, consultationId);

        // Повідомити другого учасника
        await Clients.OthersInGroup(consultationId).SendAsync("UserOnline", new
        {
            UserId = userId,
            IsOnline = true,
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Користувач закрив чат/вийшов з консультації
    /// Повідомляє другого користувача що він оффлайн
    /// </summary>
    public async Task LeaveConsultation(string consultationId)
    {
        var userId = GetUserId();
        
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, consultationId);
        
        _logger.LogInformation(
            "User {UserId} closed consultation {ConsultationId}",
            userId, consultationId);

        await Clients.OthersInGroup(consultationId).SendAsync("UserOnline", new
        {
            UserId = userId,
            IsOnline = false,
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Відправити повідомлення в консультацію
    /// receiverId визначається автоматично на основі консультації
    /// </summary>
    public async Task SendMessage(string consultationId, string text, 
        List<AttachmentData>? attachments = null)
    {
        try
        {
            var senderId = GetUserId();

            if (string.IsNullOrEmpty(senderId))
            {
                await Clients.Caller.SendAsync("Error", "User not authenticated");
                return;
            }

            // Створити command для відправки повідомлення
            var command = new SendMessageCommand(
                ConsultationId: Guid.Parse(consultationId),
                SenderId: Guid.Parse(senderId),
                Text: text,
                MediaPaths: null
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
                            var senderOption = await _userQuery.GetByIdAsync(fullMessage.SenderId, CancellationToken.None);
                            var receiverOption = await _userQuery.GetByIdAsync(fullMessage.ReceiverId, CancellationToken.None);

                            var dto = _mapper.Map<MessageDto>(fullMessage);
                            dto.FullNameSender = senderOption.Match(u => u.FullName, () => "Unknown");
                            dto.FullNameReceiver = receiverOption.Match(u => u.FullName, () => "Unknown");

                            // Відправити обом учасникам консультації
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

            // Повідомити другого користувача
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
    /// Показати індикатор що користувач друкує
    /// </summary>
    public async Task Typing(string consultationId)
    {
        var userId = GetUserId();
        
        await Clients.OthersInGroup(consultationId).SendAsync("UserTyping", new
        {
            UserId = userId,
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Сховати індикатор набору
    /// </summary>
    public async Task StopTyping(string consultationId)
    {
        var userId = GetUserId();
        
        await Clients.OthersInGroup(consultationId).SendAsync("UserStoppedTyping", new
        {
            UserId = userId
        });
    }

    /// <summary>
    /// Видалити повідомлення (тільки для власника)
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
                    // Повідомити обох учасників про видалення
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

    /// <summary>
    /// Отримати попередні повідомлення консультації
    /// </summary>
    public async Task LoadMessages(string consultationId)
    {
        try
        {
            var consultationGuid = Guid.Parse(consultationId);
            var consulId = new ConsultationsId(consultationGuid);
            
            var messagesOption = await _messageQuery.GetAllMessagesByConsultationId(consulId, CancellationToken.None);
            
            await messagesOption.Match(
                async messages =>
                {
                    var dtos = new List<MessageDto>();

                    foreach (var message in messages)
                    {
                        var senderOption = await _userQuery.GetByIdAsync(message.SenderId, CancellationToken.None);
                        var receiverOption = await _userQuery.GetByIdAsync(message.ReceiverId, CancellationToken.None);

                        var dto = _mapper.Map<MessageDto>(message);
                        dto.FullNameSender = senderOption.Match(u => u.FullName, () => "Unknown");
                        dto.FullNameReceiver = receiverOption.Match(u => u.FullName, () => "Unknown");
                        dtos.Add(dto);
                    }

                    await Clients.Caller.SendAsync("LoadMessagesResult", dtos);
                },
                async () =>
                {
                    await Clients.Caller.SendAsync("LoadMessagesResult", new List<MessageDto>());
                }
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading messages");
            await Clients.Caller.SendAsync("Error", "Failed to load messages");
        }
    }

    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        
        _logger.LogInformation("User {UserId} connected. Connection {ConnectionId}", 
            userId, Context.ConnectionId);
        
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetUserId();
        
        _logger.LogInformation("User {UserId} disconnected. Connection {ConnectionId}", 
            userId, Context.ConnectionId);
        
        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Helper для отримання userId з JWT
    /// </summary>
    private string GetUserId()
    {
        return Context.User?.FindFirst("sub")?.Value 
            ?? Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value 
            ?? string.Empty;
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
