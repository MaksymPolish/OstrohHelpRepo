using Application.Common;
using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using Application.QuestionnaireStatus.Exceptions;
using Domain.Questionnaires;
using Domain.Questionnaires.Statuses;
using MediatR;

namespace Application.QuestionnaireStatus.Commands;

public record UpdateQuestionaryStatusCommand(Guid Id, string Name) : IRequest<Result<QuestionnaireStatuses, QuestionnaireException>>;

public class UpdateQuestionaryStatusHandler(IQuestionnaireStatusRepository repository, IQuestionnaireStatusQuery query)
    : IRequestHandler<UpdateQuestionaryStatusCommand, Result<QuestionnaireStatuses, QuestionnaireException>>
{
    public async Task<Result<QuestionnaireStatuses, QuestionnaireException>> Handle(UpdateQuestionaryStatusCommand command, CancellationToken ct)
    {
        var statusId = new QuestionnaireStatusesId(command.Id);
        var existing = await query.GetByIdAsync(statusId, ct);

        return await existing.Match(
            async s =>
            {
                s.Name = command.Name;
                return await repository.UpdateAsync(s, ct);
            },
            () => Task.FromResult<Result<QuestionnaireStatuses, QuestionnaireException>>(
                new QuestionaryNotFoundException(statusId)
            )
        );
    }
}