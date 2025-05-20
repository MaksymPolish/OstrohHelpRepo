using Application.ConsultationStatus.Exceptions;
using Domain.Conferences.Statuses;
using Optional;

namespace Application.Common.Interfaces.Repositories;

public interface IConsultationStatusRepository
{
    Task AddAsync(ConsultationStatuses status, CancellationToken ct);
    
    Task<Result<ConsultationStatuses, ConsultationStatusExceptions>> UpdateAsync (ConsultationStatuses status, CancellationToken ct);
    
    Task<Result<ConsultationStatuses, ConsultationStatusExceptions>> DeleteAsync(ConsultationStatuses status, CancellationToken ct);
}