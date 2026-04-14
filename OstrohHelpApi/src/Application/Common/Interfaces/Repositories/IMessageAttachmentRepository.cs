using Domain.Messages;

namespace Application.Common.Interfaces.Repositories;

public interface IMessageAttachmentRepository
{
    Task<MessageAttachment> AddAsync(MessageAttachment attachment, CancellationToken ct);
    Task<List<MessageAttachment>> GetByMessageIdAsync(MessageId messageId, CancellationToken ct);
    Task<MessageAttachment?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<List<MessageAttachment>> GetStandaloneAttachmentsAsync(CancellationToken ct);
    Task UpdateAsync(MessageAttachment attachment, CancellationToken ct);
    Task DeleteAsync(MessageAttachment attachment, CancellationToken ct);
}
