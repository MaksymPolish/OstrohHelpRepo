using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using Domain.Users.Roles;
using Microsoft.EntityFrameworkCore;
using Optional;

namespace Infrastructure.Persistence.Repositories;

public class RoleRepository(ApplicationDbContext context) : IRoleQuery, IRoleRepository
{
    // === IRoleQuery ===
    public async Task<IEnumerable<Role>> GetAllAsync(CancellationToken ct)
    {
        return await context.Roles.ToListAsync(ct);
    }

    public async Task<Option<Role>> GetByIdAsync(RoleId roleId, CancellationToken ct)
    {
        var entity = await context.Roles
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == roleId, ct);

        return entity == null ? Option.None<Role>() : Option.Some(entity);
    }
    
    public async Task<RoleId?> GetRoleIdByNameAsync(string roleName, CancellationToken ct)
    {
        var role = await context.Roles
            .Where(r => r.Name == roleName)
            .FirstOrDefaultAsync(ct);

        return role?.Id;
    }

    // === IRoleRepository ===
    public async Task AddAsync(Role role, CancellationToken ct)
    {
        await context.Roles.AddAsync(role, ct);
        await context.SaveChangesAsync(ct);
    }

    public async Task<Role> UpdateAsync(Role role, CancellationToken ct)
    {
        context.Roles.Update(role);
        await context.SaveChangesAsync(ct);

        return role; 
    }

    public async Task<Role> DeleteAsync(Role role, CancellationToken ct)
    {
        context.Roles.Remove(role);
        await context.SaveChangesAsync(ct);
        return role;
    }
}