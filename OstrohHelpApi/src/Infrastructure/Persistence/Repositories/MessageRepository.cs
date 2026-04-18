using Application.Common;
using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using Application.Messages.Exceptions;
using Domain.Conferences;
using Domain.Messages;
using Microsoft.EntityFrameworkCore;
using Optional;

namespace Infrastructure.Persistence.Repositories;

public class MessageRepository(ApplicationDbContext context) : IMessageQuery, IMessageRepository
{
    public async Task<Result<Message, MessageExceptions>> AddAsync(Message message, CancellationToken ct)
    {
        context.Messages.Add(message);
        await context.SaveChangesAsync(ct);
        return message;
    }

    public async Task<Result<Message, MessageExceptions>> UpdateRead(Message message, CancellationToken ct)
    {
        context.Messages.Update(message);
        await context.SaveChangesAsync(ct);
        return message;
    }

    public async Task<Message> DeleteAsync(Message message, CancellationToken ct)
    {
        context.Messages.Remove(message);
        await context.SaveChangesAsync(ct);
        return message;
    }

    public async Task<Result<Message, MessageExceptions>> UpdateAsync(Message message, CancellationToken ct)
    {
        context.Messages.Update(message);
        await context.SaveChangesAsync(ct);
        return message;
    }

    public async Task<Option<List<Message>>> GetAllMessagesByConsultationId(Guid id, CancellationToken cancellationToken)
    {
        var messages = await context.Messages
            .AsNoTracking()
            .Where(x => x.ConsultationId == id)  // Return all messages, including deleted (IsDeleted flag in DTO)
            .ToListAsync(cancellationToken);
        
        if (messages.Count == 0) return Option.None<List<Message>>();

        // Manually load attachments (Attachments navigation is NotMapped)
        var messageIds = messages.Select(m => m.Id).ToList();
        var attachments = await context.Set<MessageAttachment>()
            .AsNoTracking()
            .Where(a => a.MessageId.HasValue && messageIds.Contains(a.MessageId.Value))  // Return all attachments, including deleted
            .ToListAsync(cancellationToken);

        foreach (var message in messages)
        {
            message.Attachments = attachments
                .Where(a => a.MessageId == message.Id)
                .ToList();
        }

        return Option.Some(messages);
    }

    public async Task<Option<Message>> GetMessageById(Guid id, CancellationToken cancellationToken)
    {
        var message = await context.Messages
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);  // Return message, including deleted
        
        if (message == null) return Option.None<Message>();

        // Manually load attachments (Attachments navigation is NotMapped)
        message.Attachments = await context.Set<MessageAttachment>()
            .AsNoTracking()
            .Where(a => a.MessageId == message.Id)  // Return all attachments, including deleted
            .ToListAsync(cancellationToken);

        return Option.Some(message);
    }
}