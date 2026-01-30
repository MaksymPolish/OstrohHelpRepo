using Domain.Messages;

namespace Application.Common.Interfaces.Repositories;

public interface IMessageAttachmentRepository
{
    Task<MessageAttachment> AddAsync(MessageAttachment attachment, CancellationToken ct);
    Task<List<MessageAttachment>> GetByMessageIdAsync(MessageId messageId, CancellationToken ct);
}
