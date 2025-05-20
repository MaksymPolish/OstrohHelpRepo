using Application.Common;
using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using Application.ConsultationStatus.Exceptions;
using Domain.Conferences.Statuses;
using MediatR;

namespace Application.ConsultationStatus.Commands;

public record DeleteConsultationStatusCommand(Guid Id)
    : IRequest<Result<ConsultationStatuses, ConsultationStatusExceptions>>;

public class DeleteConsultationStatusCommandHandler(
    IConsultationStatusRepository consultationStatusRepository,
    IConsultationStatusQuery consultationStatusQuery)
    : IRequestHandler<DeleteConsultationStatusCommand, Result<ConsultationStatuses, ConsultationStatusExceptions>>
{

    public async Task<Result<ConsultationStatuses, ConsultationStatusExceptions>> Handle(DeleteConsultationStatusCommand request, CancellationToken cancellationToken)
    {
        var id = new ConsultationStatusesId(request.Id);
        var consultationStatusOption = await consultationStatusQuery.GetByIdAsync(id, cancellationToken);

        return await consultationStatusOption.Match(
            async cs => await consultationStatusRepository.DeleteAsync(cs, cancellationToken),
            () => Task.FromResult<Result<ConsultationStatuses, ConsultationStatusExceptions>>(new ConsultationStatusNotFoundExceptions(id))
        );
    }

}
    
