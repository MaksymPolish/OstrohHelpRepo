using Application.Common;
using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using Application.Messages.Exceptions;
using Domain.Messages;
using MediatR;

namespace Application.Messages.Commands;

public record DeleteMessageCommand(Guid MessageId) 
    : IRequest<Result<Message, MessageExceptions>>;

public class DeleteMessageCommandHandler(IMessageQuery _messageQuery, IMessageRepository _messageRepository) : IRequestHandler<DeleteMessageCommand, Result<Message, MessageExceptions>>
{
    public async Task<Result<Message, MessageExceptions>> Handle(DeleteMessageCommand request, CancellationToken cancellationToken)
    {
        var messageid = new MessageId(request.MessageId);
        
        var messageOption = await _messageQuery.GetMessageById(messageid, cancellationToken);

        return await messageOption.Match(
            async message => await DeleteEntity(message, cancellationToken),
            () => Task.FromResult<Result<Message, MessageExceptions>>(
                new MessageNotFoundException(messageid)
            )
        );
    }
    public async Task<Result<Message, MessageExceptions>> DeleteEntity(Message message, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _messageRepository.DeleteAsync(message, cancellationToken);
            return result;
        }
        catch (Exception e)
        {
            return new MessageNotFoundException(message.Id);
        }
    } 
}
    