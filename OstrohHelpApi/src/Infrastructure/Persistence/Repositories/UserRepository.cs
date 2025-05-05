using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using Domain.Users;
using Microsoft.EntityFrameworkCore;

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
}