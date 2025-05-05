using Domain.Users.Roles;

namespace Application.Common.Interfaces.Queries;

public interface IRoleQuery
{
    Task<IEnumerable<Role>> GetAllAsync(CancellationToken ct);
    Task<Role?> GetByIdAsync(RoleId roleId, CancellationToken ct);
    Task<RoleId?> GetRoleIdByNameAsync(string roleName, CancellationToken ct);
}