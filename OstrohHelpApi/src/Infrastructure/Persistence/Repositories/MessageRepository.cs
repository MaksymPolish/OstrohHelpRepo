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

    public async Task<Option<List<Message>>> GetAllMessagesByConsultationId(ConsultationsId id, CancellationToken cancellationToken)
    {
        var entity = await context.Messages
            .AsNoTracking()
            .Include(m => m.Attachments)
            .Where(x => x.ConsultationId == id).ToListAsync(cancellationToken);
        
        return entity == null ? Option.None<List<Message>>() : Option.Some(entity);
    }

    public async Task<Option<Message>> GetMessageById(MessageId id, CancellationToken cancellationToken)
    {
        var entity = await context.Messages
            .AsNoTracking()
            .Include(m => m.Attachments)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        
        return entity == null ? Option.None<Message>() : Option.Some(entity);
    }
}