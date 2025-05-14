using Application.Common;
using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using Application.QuestionnaireStatus.Exceptions;
using Domain.Inventory.Statuses;
using MediatR;

namespace Application.QuestionnaireStatus.Commands;

public record DeleteQuestionaryStatusCommand(Guid Id) : IRequest<Result<QuestionaryStatuses, QuestionnaireStatusException>>;

public class DeleteQuestionaryStatusHandler(IQuestionnaireStatusRepository repository, IQuestionnaireStatusQuery query)
    : IRequestHandler<DeleteQuestionaryStatusCommand, Result<QuestionaryStatuses, QuestionnaireStatusException>>
{
    public async Task<Result<QuestionaryStatuses, QuestionnaireStatusException>> Handle(DeleteQuestionaryStatusCommand command, CancellationToken ct)
    {
        var statusId = new questionaryStatusId(command.Id);
        
        var entity = await query.GetByIdAsync(statusId, ct);

        return await entity.Match(
            async s =>
            {
                return await DeleteEntity(s, ct);
            },
            () => Task.FromResult<Result<QuestionaryStatuses, QuestionnaireStatusException>>(
                new QuestionaryNotFoundException(statusId)
            )
        );
    }
    
    public async Task<Result<QuestionaryStatuses, QuestionnaireStatusException>> DeleteEntity (QuestionaryStatuses status, CancellationToken ct)
    {
        try
        {
            var deletedEntity = await repository.DeleteAsync(status, ct);

            return deletedEntity;
        }
        catch (Exception e)
        {
            throw new QuestionaryNotFoundException(status.Id);
        }
    }
}