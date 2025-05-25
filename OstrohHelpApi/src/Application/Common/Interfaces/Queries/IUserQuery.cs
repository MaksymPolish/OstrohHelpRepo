using Domain.Users;
using Optional;

namespace Application.Common.Interfaces.Queries;

public interface IUserQuery
{
    Task<Option<User>> GetByIdAsync(UserId userId, CancellationToken ct);
    
    Task<IReadOnlyList<User>> GetAllAsync(CancellationToken ct);

    Task<Option<User>> GetByEmailAsync(string email, CancellationToken ct);
}