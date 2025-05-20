using Application.Common;
using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using Application.ConsultationStatus.Exceptions;
using Domain.Conferences.Statuses;
using MediatR;

namespace Application.ConsultationStatus.Commands;

public record UpdateConsultationStatusCommand(Guid Id, string Name) : IRequest<Result<ConsultationStatuses, ConsultationStatusExceptions>>;

public class UpdateConsultationStatusCommandHandler(IConsultationStatusRepository _consultationStatusRepository, 
    IConsultationStatusQuery _consultationStatusQuery) 
    : IRequestHandler<UpdateConsultationStatusCommand, Result<ConsultationStatuses, ConsultationStatusExceptions>>
{
    public async Task<Result<ConsultationStatuses, ConsultationStatusExceptions>> Handle(UpdateConsultationStatusCommand request, CancellationToken cancellationToken)
    {
        var id = new ConsultationStatusesId(request.Id);
        var consultationStatusOption = await _consultationStatusQuery.GetByIdAsync(id, cancellationToken);
        
        return await consultationStatusOption.Match(
            async cs => 
            {
                cs.Name = request.Name;
                var result = await _consultationStatusRepository.UpdateAsync(cs, cancellationToken);
                
                return result;
            },
            () => Task.FromResult<Result<ConsultationStatuses, ConsultationStatusExceptions>>(new ConsultationStatusNotFoundExceptions(id))
        );
    }
} 