using Domain.Inventory;
using Domain.Users;
using Optional;

namespace Application.Common.Interfaces.Queries;

public interface IQuestionnaireQuery
{
    Task<IEnumerable<Questionary>> GetAllAsync(CancellationToken ct);
    
    Task<Option<Questionary>> GetByIdAsync(QuestionaryId id, CancellationToken ct);
    
    Task<Option<Questionary>> GetByUserIdAsync(UserId id, CancellationToken ct);
    
    Task<IEnumerable<Questionary>> GetAllByUserIdAsync(UserId id, CancellationToken ct);
}