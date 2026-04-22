using Api.Dtos;
using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using Application.Common.Services;
using Application.Messages.Commands;
using AutoMapper;
using Domain.Conferences;
using Domain.Messages;
using Domain.Users;
using Infrastructure.Encryption;
using Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using DotNetEnv;
using Api.Services;

namespace Api.Hubs;

/// SignalR Hub для real-time чату консультацій
/// 
/// Модель: 1-на-1 консультація (1 студент + 1 психолог)
/// Чат створюється автоматично коли психолог приймає анкету
/// 
/// Потоки даних:
/// - При загрузці екрану чату -> JoinConsultation(consultationId)
/// - При закриvassureEguюч екрану -> LeaveConsultation(consultationId)
/// - При відправці -> SendMessage(consultationId, text, attachments)
[Authorize]
public class ChatHub : Hub
{
    private readonly IMediator _mediator;
    private readonly IMessageQuery _messageQuery;
    private readonly IMapper _mapper;
    private readonly IUserQuery _userQuery;
    private readonly ILogger<ChatHub> _logger;
    private readonly IMessageAttachmentRepository _attachmentRepository;
    private readonly IConsultationAccessChecker _accessChecker;
    private readonly IEncryptionService _encryptionService;
    private readonly IKeyDerivationService _keyDerivationService;
    private readonly IAuditLogService _auditLogService;
    private readonly IPresenceTracker _presenceTracker;
    private readonly ApplicationDbContext _dbContext;

    public ChatHub(
        IMediator mediator,
        IMessageQuery messageQuery,
        IMapper mapper,
        IUserQuery userQuery,
        ILogger<ChatHub> logger,
        IMessageAttachmentRepository attachmentRepository,
        IConsultationAccessChecker accessChecker,
        IEncryptionService encryptionService,
        IKeyDerivationService keyDerivationService,
        IAuditLogService auditLogService,
        IPresenceTracker presenceTracker,
        ApplicationDbContext dbContext)
    {
        _mediator = mediator;
        _messageQuery = messageQuery;
        _mapper = mapper;
        _userQuery = userQuery;
        _logger = logger;
        _attachmentRepository = attachmentRepository;
        _accessChecker = accessChecker;
        _encryptionService = encryptionService;
        _keyDerivationService = keyDerivationService;
        _auditLogService = auditLogService;
        _presenceTracker = presenceTracker;
        _dbContext = dbContext;
    }

    /// Користувач відкрив чат консультації
    /// Автоматично приєднується до групи консультації
    /// Повідомляє другого користувача що він онлайн
    /// Передає інкрипційний ключ консультації клієнту
    public async Task JoinConsultation(string consultationId)
    {
        var userId = GetUserId();
        
        // SECURITY: Перевірити, чи користувач є членом цієї консультації
        var consultationGuid = Guid.Parse(consultationId);
        var isMember = await _accessChecker.IsConsultationMember(Guid.Parse(userId), consultationGuid, CancellationToken.None);
        
        if (!isMember)
        {
            await Clients.Caller.SendAsync("Error", "You are not a member of this consultation");
            _logger.LogWarning(
                "Unauthorized access attempt: User {UserId} tried to join consultation {ConsultationId}",
                userId, consultationId);
            return;
        }
        
        await Groups.AddToGroupAsync(Context.ConnectionId, consultationId);
        
        _logger.LogInformation(
            "User {UserId} opened consultation {ConsultationId}",
            userId, consultationId);

        // Send encryption key to client
        // The client will use this key to decrypt messages
        var consultationKey = _keyDerivationService.DeriveKeyForConsultation(
            GetMasterKeyFromEnvironment(), 
            consultationGuid);
        var keyBase64 = Convert.ToBase64String(consultationKey);
        
        await Clients.Caller.SendAsync("ReceiveConsultationKey", new
        {
            ConsultationId = consultationId,
            Key = keyBase64,
            Algorithm = "AES-256-GCM",
            Timestamp = DateTime.UtcNow
        });

        // Повідомити другого учасника
        await Clients.OthersInGroup(consultationId).SendAsync("UserOnline", new
        {
            UserId = userId,
            IsOnline = true,
            Timestamp = DateTime.UtcNow
        });
    }

    /// Користувач закрив чат/вийшов з консультації
    /// Повідомляє другого користувача що він оффлайн
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
    /// Send an already-encrypted message to a consultation.
    /// The message is encrypted on client side before being sent.
    /// </summary>
    /// <param name="consultationId">Consultation identifier</param>
    /// <param name="encryptedContentBase64">Encrypted message content (base64 encoded)</param>
    /// <param name="ivBase64">Initialization vector (base64 encoded)</param>
    /// <param name="authTagBase64">Authentication tag (base64 encoded)</param>
    /// <param name="attachments">Optional list of attachments</param>
    public async Task SendMessage(string consultationId, string encryptedContentBase64, 
        string ivBase64, string authTagBase64, List<AttachmentData>? attachments = null)
    {
        try
        {
            var senderId = GetUserId();

            if (string.IsNullOrEmpty(senderId))
            {
                await Clients.Caller.SendAsync("Error", "User not authenticated");
                return;
            }

            //  SECURITY: Перевірити, чи користувач є членом цієї консультації
            var consultationGuid = Guid.Parse(consultationId);
            var isMember = await _accessChecker.IsConsultationMember(
                Guid.Parse(senderId), consultationGuid, CancellationToken.None);
            
            if (!isMember)
            {
                await Clients.Caller.SendAsync("Error", 
                    "You are not a member of this consultation and cannot send messages");
                _logger.LogWarning(
                    "Unauthorized message send attempt: User {UserId} tried to send message to consultation {ConsultationId}",
                    senderId, consultationId);
                return;
            }

            // Decode base64 strings to byte arrays
            var encryptedContent = Convert.FromBase64String(encryptedContentBase64);
            var iv = Convert.FromBase64String(ivBase64);
            var authTag = Convert.FromBase64String(authTagBase64);

            // Створити command для збереження вже зашифрованого повідомлення
            var command = new SendMessageCommand(
                ConsultationId: Guid.Parse(consultationId),
                SenderId: Guid.Parse(senderId),
                EncryptedContent: encryptedContent,
                Iv: iv,
                AuthTag: authTag,
                AttachmentIds: null
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

                            // 📋 Audit log: User sent a message
                            try
                            {
                                var ipAddress = Context.GetHttpContext()?.Connection.RemoteIpAddress?.ToString();
                                var details = System.Text.Json.JsonSerializer.Serialize(new
                                {
                                    ConsultationId = consultationId,
                                    ReceiverId = fullMessage.ReceiverId,
                                    HasAttachments = fullMessage.Attachments?.Any() ?? false
                                });

                                await _auditLogService.LogAsync(
                                    Guid.Parse(senderId),
                                    "SendMessage",
                                    "Message",
                                    message.Id,
                                    ipAddress,
                                    details
                                );
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Failed to create audit log for message {MessageId}", message.Id);
                            }
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

    /// Позначити повідомлення як прочитане
    public async Task MarkAsRead(string messageId, string consultationId)
    {
        try
        {
            var userId = GetUserId();
            var msgGuid = Guid.Parse(messageId);
            
            // SECURITY: Перевірити, що користувач є отримувачем цього повідомлення
            var msgOption = await _messageQuery.GetMessageById(msgGuid, CancellationToken.None);
            
            var isAuthorized = await msgOption.Match(
                async msg =>
                {
                    // Тільки отримувач може позначити повідомлення як прочитане
                    return msg.ReceiverId == Guid.Parse(userId);
                },
                () => Task.FromResult(false)
            );
            
            if (!isAuthorized)
            {
                await Clients.Caller.SendAsync("Error", "Unauthorized: You cannot mark this message as read");
                _logger.LogWarning(
                    "Unauthorized mark as read attempt: User {UserId} tried to mark message {MessageId} as read",
                    userId, messageId);
                return;
            }

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

    /// Показати індикатор що користувач друкує
    public async Task Typing(string consultationId)
    {
        var userId = GetUserId();
        
        await Clients.OthersInGroup(consultationId).SendAsync("UserTyping", new
        {
            UserId = userId,
            Timestamp = DateTime.UtcNow
        });
    }

    /// Сховати індикатор набору
    public async Task StopTyping(string consultationId)
    {
        var userId = GetUserId();
        
        await Clients.OthersInGroup(consultationId).SendAsync("UserStoppedTyping", new
        {
            UserId = userId
        });
    }

    /// Видалити повідомлення (тільки для власника)
    public async Task DeleteMessage(string messageId, string consultationId)
    {
        try
        {
            var userId = GetUserId();
            var msgGuid = Guid.Parse(messageId);
            
            //  SECURITY: Перевірити, що користувач є власником цього повідомлення
            var isOwner = await _accessChecker.IsMessageOwner(Guid.Parse(userId), msgGuid, CancellationToken.None);
            
            if (!isOwner)
            {
                await Clients.Caller.SendAsync("Error", "Unauthorized: You can only delete your own messages");
                _logger.LogWarning(
                    "Unauthorized message delete attempt: User {UserId} tried to delete message {MessageId}",
                    userId, messageId);
                return;
            }

            var command = new DeleteMessageCommand(MessageId: msgGuid);
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

                    // 📋 Audit log: User deleted a message
                    try
                    {
                        var ipAddress = Context.GetHttpContext()?.Connection.RemoteIpAddress?.ToString();
                        var details = System.Text.Json.JsonSerializer.Serialize(new
                        {
                            ConsultationId = consultationId
                        });

                        await _auditLogService.LogAsync(
                            Guid.Parse(userId),
                            "DeleteMessage",
                            "Message",
                            msgGuid,
                            ipAddress,
                            details
                        );
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to create audit log for message deletion {MessageId}", messageId);
                    }
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

    /// Отримати попередні повідомлення консультації
    public async Task LoadMessages(string consultationId)
    {
        try
        {
            var consultationGuid = Guid.Parse(consultationId);
            
            var messagesOption = await _messageQuery.GetAllMessagesByConsultationId(consultationGuid, CancellationToken.None);
            
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

        if (!string.IsNullOrWhiteSpace(userId))
        {
            var isFirstConnection = _presenceTracker.UserConnected(userId, Context.ConnectionId);

            if (isFirstConnection)
            {
                await UpdateOnlineStatusAsync(userId, true, Context.ConnectionAborted);
                await Clients.Others.SendAsync("UserStatusChanged", userId, true, Context.ConnectionAborted);
            }
        }
        
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetUserId();

        _logger.LogInformation("User {UserId} disconnected. Connection {ConnectionId}", 
            userId, Context.ConnectionId);

        if (!string.IsNullOrWhiteSpace(userId))
        {
            var wasLastConnection = _presenceTracker.UserDisconnected(userId, Context.ConnectionId);

            if (wasLastConnection)
            {
                await UpdateOnlineStatusAsync(userId, false, CancellationToken.None);
                await Clients.Others.SendAsync("UserStatusChanged", userId, false);
            }
        }
        
        await base.OnDisconnectedAsync(exception);
    }

    private async Task UpdateOnlineStatusAsync(string userId, bool isOnline, CancellationToken ct)
    {
        if (!Guid.TryParse(userId, out var userGuid))
        {
            _logger.LogWarning("Invalid user id format in presence update: {UserId}", userId);
            return;
        }

        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userGuid, ct);
        if (user == null)
        {
            _logger.LogWarning("User not found for presence update: {UserId}", userId);
            return;
        }

        if (user.IsLoggedIn == isOnline)
        {
            return;
        }

        // Persist online/offline status in database.
        user.IsLoggedIn = isOnline;
        await _dbContext.SaveChangesAsync(ct);
    }

    /// Helper для отримання userId з JWT
    private string GetUserId()
    {
        return Context.User?.FindFirst("sub")?.Value 
            ?? Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value 
            ?? string.Empty;
    }

    /// Helper для отримання Master Key з окружения
    private byte[] GetMasterKeyFromEnvironment()
    {
        var masterKeyBase64 = DotNetEnv.Env.GetString("ENCRYPTION_MASTER_KEY")
            ?? throw new InvalidOperationException("ENCRYPTION_MASTER_KEY environment variable is not configured");
        
        try
        {
            return Convert.FromBase64String(masterKeyBase64);
        }
        catch (FormatException ex)
        {
            throw new InvalidOperationException("ENCRYPTION_MASTER_KEY must be valid base64 encoded", ex);
        }
    }
}

/// Клас для передачі даних про attachment
public class AttachmentData
{
    public string FileUrl { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
}
