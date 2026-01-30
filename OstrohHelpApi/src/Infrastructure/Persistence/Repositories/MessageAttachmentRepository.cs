using Application.Common.Interfaces.Repositories;
using Domain.Messages;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class MessageAttachmentRepository(ApplicationDbContext context) : IMessageAttachmentRepository
{
    public async Task<MessageAttachment> AddAsync(MessageAttachment attachment, CancellationToken ct)
    {
        context.MessageAttachments.Add(attachment);
        await context.SaveChangesAsync(ct);
        return attachment;
    }

    public async Task<List<MessageAttachment>> GetByMessageIdAsync(MessageId messageId, CancellationToken ct)
    {
        return await context.MessageAttachments
            .AsNoTracking()
            .Where(a => a.MessageId == messageId)
            .ToListAsync(ct);
    }
}
