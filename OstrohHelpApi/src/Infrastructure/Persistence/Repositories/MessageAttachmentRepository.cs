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

    public async Task<List<MessageAttachment>> GetByMessageIdAsync(Guid messageId, CancellationToken ct)
    {
        return await context.MessageAttachments
            .AsNoTracking()
            .Where(a => a.MessageId == messageId)
            .ToListAsync(ct);
    }

    public async Task<MessageAttachment?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await context.MessageAttachments
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id, ct);
    }

    public async Task<List<MessageAttachment>> GetStandaloneAttachmentsAsync(CancellationToken ct)
    {
        return await context.MessageAttachments
            .AsNoTracking()
            .Where(a => a.MessageId == null)
            .ToListAsync(ct);
    }

    public async Task<List<MessageAttachment>> GetOrphanedAttachmentsAsync(DateTimeOffset cutoffDate, CancellationToken ct)
    {
        return await context.MessageAttachments
            .AsNoTracking()
            .Where(a => a.MessageId == null && a.CreatedAt < cutoffDate)
            .ToListAsync(ct);
    }

    public async Task UpdateAsync(MessageAttachment attachment, CancellationToken ct)
    {
        context.MessageAttachments.Update(attachment);
        await context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(MessageAttachment attachment, CancellationToken ct)
    {
        context.MessageAttachments.Remove(attachment);
        await context.SaveChangesAsync(ct);
    }
}
