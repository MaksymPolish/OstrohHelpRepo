using Application.Messages.Exceptions;
using Domain.Messages;

namespace Application.Common.Interfaces.Repositories;

public interface IMessageRepository
{
    Task<Result<Message, MessageExceptions>> AddAsync(Message message, CancellationToken ct);
    
    Task<Result<Message, MessageExceptions>> UpdateRead(Message message, CancellationToken ct);
    
    Task<Message> DeleteAsync(Message message, CancellationToken ct);
}