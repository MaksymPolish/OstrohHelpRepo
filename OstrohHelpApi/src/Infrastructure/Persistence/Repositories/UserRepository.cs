using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using Optional;

namespace Infrastructure.Persistence.Repositories;

public class UserRepository(ApplicationDbContext context) : IUserQuery, IUserRepository
{
    public async Task<User?> GetByGoogleIdOrEmailAsync(string googleId, string email, CancellationToken ct)
    {
        return await context.Users
            .FirstOrDefaultAsync(u => u.GoogleId == googleId || u.Email == email, ct);
    }

    public async Task AddAsync(User user, CancellationToken ct)
    {
        await context.Users.AddAsync(user, ct);
        await context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(User user, CancellationToken ct)
    {
        context.Users.Update(user);
        await context.SaveChangesAsync(ct);
    }

    public async Task<User> DeleteAsync(User user, CancellationToken ct)
    {
        context.Users.Remove(user);
        
        await context.SaveChangesAsync(ct); 
        
        return user;
    }

    public async Task<Option<User>> GetByIdAsync(UserId userId, CancellationToken ct)
    {
        var entity = await context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == userId, ct);

        return entity == null ? Option.None<User>() : Option.Some(entity);
    }

    public async Task<IReadOnlyList<User>> GetAllAsync(CancellationToken ct)
    {
        return await context
            .Users
            .AsNoTracking()
            .ToListAsync(ct); 
    }
}