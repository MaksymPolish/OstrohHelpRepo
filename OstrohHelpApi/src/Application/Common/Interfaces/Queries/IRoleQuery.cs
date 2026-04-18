using Domain.Users.Roles;
using Optional;

namespace Application.Common.Interfaces.Queries;

public interface IRoleQuery
{
    Task<IEnumerable<Role>> GetAllAsync(CancellationToken ct);
    Task<Option<Role>> GetByIdAsync(Guid roleId, CancellationToken ct);
    Task<Guid?> GetRoleIdByNameAsync(string roleName, CancellationToken ct);
}