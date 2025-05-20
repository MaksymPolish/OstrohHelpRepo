using Application.ConsultationStatus.Exceptions;
using Domain.Conferences.Statuses;
using Optional;

namespace Application.Common.Interfaces.Queries;

public interface IConsultationStatusQuery
{
    Task<IEnumerable<ConsultationStatuses>> GetAllAsync(CancellationToken cancellationToken);
    
    Task<Option<ConsultationStatuses>> GetByIdAsync(ConsultationStatusesId id, CancellationToken cancellationToken);
    
    //get by name
    Task<Option<ConsultationStatuses>> GetByNameAsync(string name, CancellationToken cancellationToken);
}