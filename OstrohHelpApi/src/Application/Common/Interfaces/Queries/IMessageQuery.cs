using Domain.Conferences;
using Domain.Messages;
using Optional;

namespace Application.Common.Interfaces.Queries;

public interface IMessageQuery
{
    Task<Option<List<Message>>> GetAllMessagesByConsultationId(Guid id, CancellationToken cancellationToken);

    Task<Option<Message>> GetMessageById(Guid id, CancellationToken cancellationToken);
}