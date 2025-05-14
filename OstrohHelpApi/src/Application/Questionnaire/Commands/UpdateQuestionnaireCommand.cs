using Application.Common;
using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using Application.Questionnaire.Exceptions;
using Domain.Inventory;
using MediatR;

namespace Application.Questionnaire.Commands;

public record UpdateQuestionnaireCommand(
    Guid Id, 
    string Description, 
    bool IsAnonymous, 
    DateTime? SubmittedAt) 
    : IRequest<Result<Questionary, QuestionnairesException>>;

public class UpdateQuestionnaireCommandHandler(IQuestionnaireQuery _questionnaireQuery, 
    IQuestionnaireRepository _questionnaireRepository) 
    : IRequestHandler<UpdateQuestionnaireCommand, Result<Questionary, QuestionnairesException>>
{
    public async Task<Result<Questionary, QuestionnairesException>> Handle(UpdateQuestionnaireCommand command, CancellationToken ct)
    {
        var questionaryId = new QuestionaryId(command.Id);
        
        var existingQuestionary = await _questionnaireQuery.GetByIdAsync(questionaryId, ct);
        
        return await existingQuestionary.Match(
            async q =>
            {
                q.Description = command.Description;
                q.IsAnonymous = command.IsAnonymous;
                q.SubmittedAt = command.SubmittedAt ?? DateTime.UtcNow;
                return await _questionnaireRepository.UpdateAsync(q, ct);
            },
            () =>
            {
                return Task.FromResult<Result<Questionary, QuestionnairesException>>(
                    new QuestionnaireNotFoundException(questionaryId)
                );
            }
        );
    }
}