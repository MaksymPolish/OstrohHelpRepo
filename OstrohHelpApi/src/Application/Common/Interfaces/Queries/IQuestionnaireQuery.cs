using Domain.Inventory;
using Domain.Users;
using Optional;

namespace Application.Common.Interfaces.Queries;

public interface IQuestionnaireQuery
{
    Task<IEnumerable<Questionary>> GetAllAsync(CancellationToken ct);
    
    /// Отримати всі анкети з деталями (User, Status) - вирішує N+1
    Task<IEnumerable<Questionary>> GetAllWithDetailsAsync(CancellationToken ct);
    
    Task<Option<Questionary>> GetByIdAsync(QuestionaryId id, CancellationToken ct);
    
    /// Отримати анкету за ID з деталями - вирішує N+1
    Task<Option<Questionary>> GetByIdWithDetailsAsync(QuestionaryId id, CancellationToken ct);
    
    Task<Option<Questionary>> GetByUserIdAsync(UserId id, CancellationToken ct);
    
    Task<IEnumerable<Questionary>> GetAllByUserIdAsync(UserId id, CancellationToken ct);
    
    /// <summary>
    /// Отримати всі анкети користувача з деталями - вирішує N+1
    /// </summary>
    Task<IEnumerable<Questionary>> GetAllByUserIdWithDetailsAsync(UserId id, CancellationToken ct);
}