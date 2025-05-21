using Application.Common;
using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using Application.Messages.Exceptions;
using Domain.Messages;
using MediatR;

namespace Application.Messages.Commands;

public record MarkMessageAsReadCommand(Guid MessageId) 
    : IRequest<Result<Message, MessageExceptions>>;
    
    
public class MarkMessageAsReadHandler(IMessageRepository _messageRepository, IMessageQuery _messageQuery)
    : IRequestHandler<MarkMessageAsReadCommand, Result<Message, MessageExceptions>>
{
    public async Task<Result<Message, MessageExceptions>> Handle(MarkMessageAsReadCommand command, CancellationToken ct)
    {
        var messageId = new MessageId(command.MessageId);
        
        var messageOption = await _messageQuery.GetMessageById(messageId, ct);

        return await messageOption.Match(
            async message =>
            {
                // --- Оновлення IsRead ---
                message.MarkAsRead();
                return await _messageRepository.UpdateRead(message, ct);
            },
            () => Task.FromResult<Result<Message, MessageExceptions>>(
                new MessageNotFoundException(messageId)
            )
        );
    }
}