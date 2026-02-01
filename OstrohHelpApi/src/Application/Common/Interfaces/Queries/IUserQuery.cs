using Domain.Users;
using Optional;

namespace Application.Common.Interfaces.Queries;

public interface IUserQuery
{
    Task<Option<User>> GetByIdAsync(UserId userId, CancellationToken ct);
    
    /// Отримати користувача з роллю за ID - вирішує N+1
    Task<Option<User>> GetByIdWithRoleAsync(UserId userId, CancellationToken ct);
    
    Task<IReadOnlyList<User>> GetAllAsync(CancellationToken ct);
    
    /// Отримати всіх користувачів разом з ролями (вирішує N+1 проблему)
    Task<IReadOnlyList<User>> GetAllWithRolesAsync(CancellationToken ct);

    Task<Option<User>> GetByEmailAsync(string email, CancellationToken ct);
    
    /// Отримати користувача з роллю за email - вирішує N+1
    Task<Option<User>> GetByEmailWithRoleAsync(string email, CancellationToken ct);
}