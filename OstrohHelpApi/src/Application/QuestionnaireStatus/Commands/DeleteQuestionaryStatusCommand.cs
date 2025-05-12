using Application.Common;
using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using Application.QuestionnaireStatus.Exceptions;
using Domain.Questionnaires;
using Domain.Questionnaires.Statuses;
using MediatR;

namespace Application.QuestionnaireStatus.Commands;

public record DeleteQuestionaryStatusCommand(Guid Id) : IRequest<Result<QuestionnaireStatuses, QuestionnaireException>>;

public class DeleteQuestionaryStatusHandler(IQuestionnaireStatusRepository repository, IQuestionnaireStatusQuery query)
    : IRequestHandler<DeleteQuestionaryStatusCommand, Result<QuestionnaireStatuses, QuestionnaireException>>
{
    public async Task<Result<QuestionnaireStatuses, QuestionnaireException>> Handle(DeleteQuestionaryStatusCommand command, CancellationToken ct)
    {
        var statusId = new QuestionnaireStatusesId(command.Id);
        
        var entity = await query.GetByIdAsync(statusId, ct);

        return await entity.Match(
            async s =>
            {
                return await DeleteEntity(s, ct);
            },
            () => Task.FromResult<Result<QuestionnaireStatuses, QuestionnaireException>>(
                new QuestionaryNotFoundException(statusId)
            )
        );
    }
    
    public async Task<Result<QuestionnaireStatuses, QuestionnaireException>> DeleteEntity (QuestionnaireStatuses status, CancellationToken ct)
    {
        try
        {
            var deletedEntity = await repository.DeleteAsync(status, ct);

            return deletedEntity;
        }
        catch (Exception e)
        {
            throw new Exception("Something go wrong with deleting questionary status", e);
        }
    }
}