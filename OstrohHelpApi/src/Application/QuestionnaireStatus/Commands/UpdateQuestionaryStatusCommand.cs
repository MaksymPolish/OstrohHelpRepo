using Application.Common;
using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using Application.QuestionnaireStatus.Exceptions;
using Domain.Inventory.Statuses;
using MediatR;

namespace Application.QuestionnaireStatus.Commands;

public record UpdateQuestionaryStatusCommand(Guid Id, string Name) : IRequest<Result<QuestionaryStatuses, QuestionnaireStatusException>>;

public class UpdateQuestionaryStatusHandler(IQuestionnaireStatusRepository repository, IQuestionnaireStatusQuery query)
    : IRequestHandler<UpdateQuestionaryStatusCommand, Result<QuestionaryStatuses, QuestionnaireStatusException>>
{
    public async Task<Result<QuestionaryStatuses, QuestionnaireStatusException>> Handle
        (UpdateQuestionaryStatusCommand command, CancellationToken ct)
    {
        var statusId = new questionaryStatusId(command.Id);
        var existing = await query.GetByIdAsync(statusId, ct);

        return await existing.Match(
            async s =>
            {
                s.Name = command.Name;
                return await repository.UpdateAsync(s, ct);
            },
            () => Task.FromResult<Result<QuestionaryStatuses, QuestionnaireStatusException>>(
                new QuestionaryNotFoundException(statusId)
            )
        );
    }
}