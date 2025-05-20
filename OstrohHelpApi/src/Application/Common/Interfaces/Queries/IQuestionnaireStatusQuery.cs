using Domain.Inventory.Statuses;
using Optional;

namespace Application.Common.Interfaces.Queries;

public interface IQuestionnaireStatusQuery
{
    Task<Option<QuestionaryStatuses>> GetByIdAsync(questionaryStatusId id, CancellationToken ct);
    Task<IEnumerable<QuestionaryStatuses>> GetAllAsync(CancellationToken ct);
    
    Task<Option<QuestionaryStatuses>> GetByNameAsync(string name, CancellationToken ct);
}