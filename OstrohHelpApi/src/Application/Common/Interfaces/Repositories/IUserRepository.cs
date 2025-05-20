using Domain.Users;

namespace Application.Common.Interfaces.Repositories;

public interface IUserRepository
{
    Task<User?> GetByGoogleIdOrEmailAsync(string googleId, string email, CancellationToken ct);
    Task AddAsync(User user, CancellationToken ct);
    Task UpdateAsync(User user, CancellationToken ct);
    Task<User> DeleteAsync(User user, CancellationToken ct);
}