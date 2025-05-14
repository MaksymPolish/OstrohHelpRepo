using Application.Questionnaire.Exceptions;
using Application.QuestionnaireStatus.Exceptions;
using Domain.Inventory;

namespace Application.Common.Interfaces.Repositories;

public interface IQuestionnaireRepository
{
    Task AddAsync(Questionary questionary, CancellationToken ct);
    
    Task<Result<Questionary, QuestionnairesException>> UpdateAsync(Questionary questionary, CancellationToken ct);
    
    Task<Questionary> DeleteAsync(Questionary questionary, CancellationToken ct);
}