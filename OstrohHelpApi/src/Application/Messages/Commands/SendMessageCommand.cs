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


/// Command to store an already-encrypted message.
/// The message is encrypted on the client side before being sent to the server.
/// The server receives the encrypted content, IV, and auth tag and stores them as-is.
public record SendMessageCommand(
    Guid ConsultationId,
    Guid SenderId,
    byte[] EncryptedContent,
    byte[] Iv,
    byte[] AuthTag,
    List<string>? MediaPaths = null) 
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

        var consultationOption = await _consultationQuery.GetByIdAsync(consultationId, ct);

        return await consultationOption.Match(
            async consultation =>
            {
                // --- Визначення отримувача ---
                var receiverId = consultation.StudentId == senderId 
                    ? consultation.PsychologistId 
                    : consultation.StudentId;
                
                // --- Створення зашифрованого повідомлення ---
                // (Encryption already done on client side, server just stores the encrypted data)
                var message = Message.CreateEncrypted(
                    id: new MessageId(Guid.NewGuid()),
                    consultationId: consultation.Id,
                    senderId: senderId,
                    receiverId: receiverId,
                    encryptedContent: command.EncryptedContent,
                    iv: command.Iv,
                    authTag: command.AuthTag,
                    sentAt: DateTime.UtcNow
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
