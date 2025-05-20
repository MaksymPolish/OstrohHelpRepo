using Application.Consultations.Exceptions;

namespace Application.Common.Interfaces.Repositories;

public interface IConsultationRepository
{
    Task<Result<Domain.Conferences.Consultations, ConsultationsExceptions>> AddAsync(Domain.Conferences.Consultations consultation, CancellationToken cancellationToken);
    
    Task<Result<Domain.Conferences.Consultations, ConsultationsExceptions>> UpdateAsync(Domain.Conferences.Consultations consultation, CancellationToken cancellationToken);
    
    Task<Result<Domain.Conferences.Consultations, ConsultationsExceptions>> DeleteAsync(Domain.Conferences.Consultations consultation, CancellationToken cancellationToken);
}