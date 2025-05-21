using Domain.Conferences;
using Domain.Messages;
using Optional;

namespace Application.Common.Interfaces.Queries;

public interface IMessageQuery
{
    Task<Option<List<Message>>> GetAllMessagesByConsultationId(ConsultationsId id, CancellationToken cancellationToken);

    Task<Option<Message>> GetMessageById(MessageId id, CancellationToken cancellationToken);
}