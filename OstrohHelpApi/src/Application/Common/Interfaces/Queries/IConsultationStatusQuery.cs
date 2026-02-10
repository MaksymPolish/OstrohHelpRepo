using Optional;
using Domain.Conferences.Statuses;

namespace Application.Common.Interfaces.Queries;

public interface IConsultationStatusQuery
{
    Task<Option<List<ConsultationStatuses>>> GetAllAsync(CancellationToken cancellationToken);
    
    Task<Option<ConsultationStatuses>> GetByIdAsync(ConsultationStatusesId id, CancellationToken cancellationToken);
    
    Task<Option<ConsultationStatuses>> GetByEnumAsync(ConsultationStatusEnum status, CancellationToken cancellationToken);

    //get by name
    Task<Option<ConsultationStatuses>> GetByNameAsync(string name, CancellationToken cancellationToken);
}