using Application.Common;
using Application.Common.Interfaces.Repositories;
using Application.QuestionnaireStatus.Exceptions;
using Domain.Inventory.Statuses;
using MediatR;

namespace Application.QuestionnaireStatus.Commands;

public record CreateQuestionaryStatusCommand(string Name)
    : IRequest<Result<QuestionaryStatuses, QuestionnaireStatusException>>;

public class CreateQuestionaryStatusHandler(IQuestionnaireStatusRepository _repository)
    : IRequestHandler<CreateQuestionaryStatusCommand, Result<QuestionaryStatuses, QuestionnaireStatusException>>
{
    public async Task<Result<QuestionaryStatuses, QuestionnaireStatusException>> Handle(CreateQuestionaryStatusCommand command, CancellationToken ct)
    {
        try
        {
            var status = QuestionaryStatuses.Create(questionaryStatusId.New(), command.Name);
        
            await _repository.AddAsync(status, ct);
        
            return status;
        }
        catch (Exception e)
        {
            throw new Exception("Something go wrong with creating questionary status", e);
        }
    }
}