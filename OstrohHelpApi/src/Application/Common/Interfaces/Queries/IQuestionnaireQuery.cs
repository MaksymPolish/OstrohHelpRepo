using Domain.Inventory;
using Domain.Users;
using Optional;

namespace Application.Common.Interfaces.Queries;

public interface IQuestionnaireQuery
{
    Task<IEnumerable<Questionary>> GetAllAsync(CancellationToken ct);
    
    /// Отримати всі анкети з деталями (User, Status) - вирішує N+1
    Task<IEnumerable<Questionary>> GetAllWithDetailsAsync(CancellationToken ct);
    
    Task<Option<Questionary>> GetByIdAsync(Guid id, CancellationToken ct);
    
    /// Отримати анкету за ID з деталями - вирішує N+1
    Task<Option<Questionary>> GetByIdWithDetailsAsync(Guid id, CancellationToken ct);
    
    Task<Option<Questionary>> GetByUserIdAsync(Guid id, CancellationToken ct);
    
    Task<IEnumerable<Questionary>> GetAllByUserIdAsync(Guid id, CancellationToken ct);
    
    /// Отримати всі анкети користувача з деталями - вирішує N+1
    Task<IEnumerable<Questionary>> GetAllByUserIdWithDetailsAsync(Guid id, CancellationToken ct);
}