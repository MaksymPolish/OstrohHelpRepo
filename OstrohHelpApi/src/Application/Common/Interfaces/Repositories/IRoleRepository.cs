using Domain.Users.Roles;

namespace Application.Common.Interfaces.Repositories;

public interface IRoleRepository
{
    Task AddAsync(Role role, CancellationToken ct);
    Task<Role> UpdateAsync(Role role, CancellationToken ct);
    Task<Role> DeleteAsync(Role role, CancellationToken ct);
}