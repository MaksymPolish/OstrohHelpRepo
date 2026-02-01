using Application.Common;
using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using Application.Questionnaire.Exceptions;
using Domain.Inventory;
using Domain.Inventory.Statuses;
using Domain.Users;
using MediatR;

namespace Application.Questionnaire.Commands;

public record CreateQuestionnaireCommand(
    Guid UserId,
    string Description,
    bool IsAnonymous,
    DateTime SubmittedAt) : IRequest<Result<Questionary, QuestionnairesException>>;

public class CreateQuestionnaireCommandHandler(IQuestionnaireRepository _repository, IQuestionnaireStatusQuery _repositoryStatusQuery)
    : IRequestHandler<CreateQuestionnaireCommand, Result<Questionary, QuestionnairesException>>
{
    public async Task<Result<Questionary, QuestionnairesException>> Handle(
        CreateQuestionnaireCommand command, 
        CancellationToken ct)
    {
        const string statusName = "Обробляється";
        var questionaryStatus = await _repositoryStatusQuery.GetByNameAsync(statusName, ct);

        if (!questionaryStatus.HasValue)
        {
            return new QuestionnaireStatusNotFoundException(statusName);
        }

        var status = questionaryStatus.Match(
            some: s => s,
            none: () => throw new QuestionnaireStatusNotFoundException(statusName));

        try
        {
            var questionnaire = Questionary.Create(
                id: QuestionaryId.New(),
                userId: new UserId(command.UserId),
                statusId: status!.Id,
                description: command.Description,
                isAnonymous: command.IsAnonymous,
                submittedAt: command.SubmittedAt
            );

            await _repository.AddAsync(questionnaire, ct);

            return questionnaire;
        }
        catch (Exception ex)
        {
            return new QuestionnaireUnknownException(QuestionaryId.New(), ex);
        }
    }
}