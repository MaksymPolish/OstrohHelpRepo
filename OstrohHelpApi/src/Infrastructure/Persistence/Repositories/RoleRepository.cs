using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using Domain.Users.Roles;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class RoleRepository(ApplicationDbContext context) : IRoleQuery, IRoleRepository
{
    // === IRoleQuery ===
    public async Task<IEnumerable<Role>> GetAllAsync(CancellationToken ct)
    {
        return await context.Roles.ToListAsync(ct);
    }

    public async Task<Role?> GetByIdAsync(RoleId roleId, CancellationToken ct)
    {
        return await context.Roles.FirstOrDefaultAsync(r => r.Id == roleId, ct);
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

    public async Task UpdateAsync(Role role, CancellationToken ct)
    {
        context.Roles.Update(role);
        await context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(RoleId roleId, CancellationToken ct)
    {
        var existing = await context.Roles.FindAsync(new object[] { roleId }, ct);
        if (existing is not null)
        {
            context.Roles.Remove(existing);
            await context.SaveChangesAsync(ct);
        }
    }
}