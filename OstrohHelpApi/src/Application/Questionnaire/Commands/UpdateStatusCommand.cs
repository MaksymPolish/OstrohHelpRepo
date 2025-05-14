using Application.Common;
using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using Application.Questionnaire.Exceptions;
using Domain.Inventory;
using Domain.Inventory.Statuses;
using MediatR;

namespace Application.Questionnaire.Commands;

public record UpdateStatusCommand(Guid Id, Guid StatusId) : IRequest<Result<Questionary,QuestionnairesException>>;

public class UpdateStatusCommandHandler( 
    IQuestionnaireQuery _query,
    IQuestionnaireRepository _repository) 
    : IRequestHandler<UpdateStatusCommand, Result<Questionary, QuestionnairesException>>
{
    public async Task<Result<Questionary, QuestionnairesException>> Handle(UpdateStatusCommand command, CancellationToken ct)
    {
        var questionnaireId = new QuestionaryId(command.Id);
        var statusId = new questionaryStatusId(command.StatusId);

        var questionnaireOption = await _query.GetByIdAsync(questionnaireId, ct);

        return await questionnaireOption.Match(
            async q =>
            {
                q.StatusId = statusId;
                return await _repository.UpdateAsync(q, ct);
            },
            () => Task.FromResult<Result<Questionary, QuestionnairesException>>(
                new QuestionnaireNotFoundException(questionnaireId)
            )
        );
    }
    
}
