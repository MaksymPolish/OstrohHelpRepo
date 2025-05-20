using Application.Common;
using Application.Common.Interfaces.Repositories;
using Application.ConsultationStatus.Exceptions;
using Domain.Conferences.Statuses;
using MediatR;

namespace Application.ConsultationStatus.Commands;

public record CreateConsultationStatusCommand(string Name) : IRequest<Result<ConsultationStatuses, ConsultationStatusExceptions>>;

public class CreateConsultationStatusCommandHandler(IConsultationStatusRepository _consultationStatusRepository) : IRequestHandler<CreateConsultationStatusCommand, Result<ConsultationStatuses, ConsultationStatusExceptions>>
{
    public async Task<Result<ConsultationStatuses, ConsultationStatusExceptions>> Handle(CreateConsultationStatusCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = ConsultationStatusesId.New();
            var consultationStatus = ConsultationStatuses.Create(id, request.Name);
            await _consultationStatusRepository.AddAsync(consultationStatus, cancellationToken);
            
            return consultationStatus;
        }
        catch (Exception e)
        {
            throw new Exception("Something go wrong with creating consultation status", e);
        }
        
    }
}