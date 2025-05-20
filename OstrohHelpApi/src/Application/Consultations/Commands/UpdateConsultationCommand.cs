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
        var consultationId = new ConsultationsId(request.consultationId);
        
        var consultation = await _consultationQuery.GetByIdAsync(consultationId, cancellationToken);

        var statusId = new ConsultationStatusesId(request.statusId);
        var studentId = new UserId(request.studentId);
        var psyhologistId = new UserId(request.psyhologistId);
        
        return await consultation.Match(
            async cn =>
            {
                cn.StatusId = statusId;
                cn.StudentId = studentId;
                cn.PsychologistId = psyhologistId;
                cn.ScheduledTime = request.scheduledTime;

                return await _consultationRepository.UpdateAsync(cn, cancellationToken);
            },
            () => Task.FromResult<Result<Domain.Conferences.Consultations, ConsultationsExceptions>>(new ConsultationNotFoundException(consultationId))
        );
    }
}