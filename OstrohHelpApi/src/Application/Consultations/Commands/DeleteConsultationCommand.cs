using Application.Common;
using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using Application.Consultations.Exceptions;
using Domain.Conferences;
using MediatR;

namespace Application.Consultations.Commands;

public record DeleteConsultationCommand(Guid id)
    : IRequest<Result<Domain.Conferences.Consultations, ConsultationsExceptions>>;

public class DeleteConsultationCommandHandler(
    IConsultationRepository _consultationRepository,
    IConsultationQuery _consultationQuery)
    : IRequestHandler<DeleteConsultationCommand, Result<Domain.Conferences.Consultations, ConsultationsExceptions>>
{
    public async Task<Result<Domain.Conferences.Consultations, ConsultationsExceptions>> Handle(DeleteConsultationCommand request, CancellationToken ct)
    {
        var id = new ConsultationsId(request.id);
        var consultationOption = await _consultationQuery.GetByIdAsync(id, ct);

        return await consultationOption.Match(
            async consultation => await DeleteEntity(consultation, ct),
            () => Task.FromResult<Result<Domain.Conferences.Consultations, ConsultationsExceptions>>(
                new ConsultationNotFoundException(id)
            )
        );
    }

    private async Task<Result<Domain.Conferences.Consultations, ConsultationsExceptions>> DeleteEntity(Domain.Conferences.Consultations consultation, CancellationToken ct)
    {
        try
        {
            var deleted = await _consultationRepository.DeleteAsync(consultation, ct);
            return deleted;
        }
        catch (Exception e)
        {
            return new SometingWrongWithConsultation(consultation.Id);
        }
    }
}
    
