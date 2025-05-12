using Application.Common;
using Application.Common.Interfaces.Repositories;
using Application.QuestionnaireStatus.Exceptions;
using Domain.Questionnaires;
using Domain.Questionnaires.Statuses;
using MediatR;

namespace Application.QuestionnaireStatus.Commands;

public record CreateQuestionaryStatusCommand(string Name)
    : IRequest<Result<QuestionnaireStatuses, QuestionnaireException>>;

public class CreateQuestionaryStatusHandler(IQuestionnaireStatusRepository _repository)
    : IRequestHandler<CreateQuestionaryStatusCommand, Result<QuestionnaireStatuses, QuestionnaireException>>
{
    public async Task<Result<QuestionnaireStatuses, QuestionnaireException>> Handle(CreateQuestionaryStatusCommand command, CancellationToken ct)
    {
        try
        {
            var status = QuestionnaireStatuses.Create(QuestionnaireStatusesId.New(), command.Name);
        
            await _repository.AddAsync(status, ct);
        
            return status;
        }
        catch (Exception e)
        {
            throw new Exception("Something go wrong with creating questionary status", e);
        }
    }
}