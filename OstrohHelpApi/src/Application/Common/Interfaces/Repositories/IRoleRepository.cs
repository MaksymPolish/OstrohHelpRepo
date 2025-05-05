using Domain.Users.Roles;

namespace Application.Common.Interfaces.Repositories;

public interface IRoleRepository
{
    Task AddAsync(Role role, CancellationToken ct);
    Task UpdateAsync(Role role, CancellationToken ct);
    Task DeleteAsync(RoleId roleId, CancellationToken ct);
}