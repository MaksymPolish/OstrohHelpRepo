using Application.Common;
using Application.Common.Interfaces.Repositories;
using Application.Questionnaire.Exceptions;
using Application.QuestionnaireStatus.Exceptions;
using Domain.Inventory;
using Domain.Inventory.Statuses;
using Domain.Users;
using MediatR;

namespace Application.Questionnaire.Commands;

public record CreateQuestionnaireCommand(
    Guid UserId,
    Guid StatusId,
    string Description,
    bool IsAnonymous,
    DateTime SubmittedAt) : IRequest<Result<Questionary, QuestionnairesException>>;

public class CreateQuestionnaireCommandHandler(IQuestionnaireRepository _repository)
    : IRequestHandler<CreateQuestionnaireCommand, Result<Questionary, QuestionnairesException>>
{
    public async Task<Result<Questionary, QuestionnairesException>> Handle(CreateQuestionnaireCommand command, CancellationToken ct)
    {
        try
        {
            var questionnaire = Questionary.Create(
                id: QuestionaryId.New(),
                userId: new UserId(command.UserId),
                statusId: new questionaryStatusId(command.StatusId),
                description: command.Description,
                isAnonymous: command.IsAnonymous,
                submittedAt: command.SubmittedAt
            );

            await _repository.AddAsync(questionnaire, ct);

            return questionnaire;
        }
        catch (Exception ex)
        {
            throw new Exception("Something go wrong with creating questionnaires", ex);
        }
    }
}