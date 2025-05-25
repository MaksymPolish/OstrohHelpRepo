using Domain.Conferences;
using Domain.Users;
using Optional;

namespace Application.Common.Interfaces.Queries;

public interface IConsultationQuery
{
    Task<IEnumerable<Domain.Conferences.Consultations>> GetAllAsync(CancellationToken ct);
    
    Task<Option<Domain.Conferences.Consultations>> GetByIdAsync(ConsultationsId id, CancellationToken ct);
    
    Task<IEnumerable<Domain.Conferences.Consultations>> GetAllByUserIdAsync(UserId id, CancellationToken ct);
}