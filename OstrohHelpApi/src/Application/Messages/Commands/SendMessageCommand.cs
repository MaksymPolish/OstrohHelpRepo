using Application.Common;
using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using Application.Consultations.Exceptions;
using Application.Messages.Exceptions;
using Domain.Conferences;
using Domain.Messages;
using Domain.Users;
using MediatR;
using ConsultationNotFoundException = Application.Messages.Exceptions.ConsultationNotFoundException;

namespace Application.Messages.Commands;

public record SendMessageCommand(
    Guid ConsultationId,
    Guid SenderId,
    Guid ReceiverId,
    string Text,
    bool IsRead = false,
    DateTime? SentAt = null) 
    : IRequest<Result<Message, MessageExceptions>>;

public class SendMessageCommandHandler(
    IConsultationQuery _consultationQuery,
    IMessageRepository _messageRepository) 
    : IRequestHandler<SendMessageCommand, Result<Message, MessageExceptions>>
{
    public async Task<Result<Message, MessageExceptions>> Handle(SendMessageCommand command, CancellationToken ct)
    {
        var consultationId = new ConsultationsId(command.ConsultationId);
        var senderId = new UserId(command.SenderId);
        var receiverId = new UserId(command.ReceiverId);

        var consultationOption = await _consultationQuery.GetByIdAsync(consultationId, ct);

        return await consultationOption.Match(
            async consultation =>
            {
                // --- Створення повідомлення ---
                var message = Message.Create(
                    id: new MessageId(Guid.NewGuid()),
                    consultationId: consultation.Id,
                    senderId: senderId,
                    receiverId: receiverId,
                    text: command.Text,
                    isRead: command.IsRead,
                    sentAt: command.SentAt ?? DateTime.UtcNow,
                    deletedAt: null
                );

                // --- Збереження через репозиторій ---
                var result = await _messageRepository.AddAsync(message, ct);

                return result;
            },
            () => Task.FromResult<Result<Message, MessageExceptions>>(
                new ConsultationNotFoundException(consultationId)
            )
        );
    }
}
