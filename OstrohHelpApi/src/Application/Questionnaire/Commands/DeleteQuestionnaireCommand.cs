using Application.Common;
using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using Application.Questionnaire.Exceptions;
using Domain.Inventory;
using MediatR;

namespace Application.Questionnaire.Commands;

public record DeleteQuestionnaireCommand(Guid Id) : IRequest<Result<Questionary, QuestionnairesException>>;

public class DeleteQuestionnaireCommandHandler(IQuestionnaireRepository _repository, IQuestionnaireQuery _query)
    : IRequestHandler<DeleteQuestionnaireCommand, Result<Questionary, QuestionnairesException>>
{
    public async Task<Result<Questionary, QuestionnairesException>> Handle(DeleteQuestionnaireCommand command, CancellationToken ct)
    {
        var entityOption = await _query.GetByIdAsync(command.Id, ct);

        return await entityOption.Match(
            async q => await DeleteEntity(q, ct),
            () => Task.FromResult<Result<Questionary, QuestionnairesException>>(
                new QuestionnaireNotFoundException(command.Id)
            )
        );
    }

    private async Task<Result<Questionary, QuestionnairesException>> DeleteEntity(Questionary questionary, CancellationToken ct)
    {
        try
        {
            var deleted = await _repository.DeleteAsync(questionary, ct);
            return deleted; 
        }
        catch (Exception ex)
        {
            return new QuestionnaireUnknownException(questionary.Id, ex);
        }
    }
}