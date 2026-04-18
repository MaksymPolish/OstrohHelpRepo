using Application.Common;
using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using Application.Consultations.Exceptions;
using Domain.Conferences;
using Domain.Conferences.Statuses;
using Domain.Users;
using MediatR;

namespace Application.Consultations.Commands;

public record UpdateConsultationCommand(Guid consultationId, Guid statusId, Guid studentId, Guid psyhologistId, DateTime scheduledTime) : IRequest<Result<Domain.Conferences.Consultations, ConsultationsExceptions>>;

public class UpdateConsultaionCommandHelper(IConsultationQuery _consultationQuery, IConsultationRepository _consultationRepository) : IRequestHandler<UpdateConsultationCommand, Result<Domain.Conferences.Consultations, ConsultationsExceptions>>
{
    public async Task<Result<Domain.Conferences.Consultations, ConsultationsExceptions>> Handle(UpdateConsultationCommand request, CancellationToken cancellationToken)
    {
        var consultation = await _consultationQuery.GetByIdAsync(request.consultationId, cancellationToken);
        
        return await consultation.Match(
            async cn =>
            {
                cn.StatusId = request.statusId;
                cn.StudentId = request.studentId;
                cn.PsychologistId = request.psyhologistId;
                cn.ScheduledTime = request.scheduledTime;

                return await _consultationRepository.UpdateAsync(cn, cancellationToken);
            },
            () => Task.FromResult<Result<Domain.Conferences.Consultations, ConsultationsExceptions>>(new ConsultationNotFoundException(request.consultationId))
        );
    }
}