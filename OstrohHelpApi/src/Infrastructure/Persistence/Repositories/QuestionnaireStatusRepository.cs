using Application.Common;
using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using Application.QuestionnaireStatus.Exceptions;
using Domain.Inventory.Statuses;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Optional;

namespace Infrastructure.Persistence.Repositories;

public class QuestionnaireStatusRepository : IQuestionnaireStatusRepository, IQuestionnaireStatusQuery
{
    private readonly ApplicationDbContext _context;
    public QuestionnaireStatusRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    [Obsolete("Use GetByEnumAsync instead")]
    public async Task<Option<QuestionaryStatuses>> GetByNameAsync(string name, CancellationToken ct)
    {
        // Map name to enum (adjust mapping as needed)
        var statusEnum = name switch
        {
            "Принято" => QuestionaryStatusEnum.Accepted,
            "Відхилено" => QuestionaryStatusEnum.Rejected,
            "Очікує підтвердження" => QuestionaryStatusEnum.Pending,
            _ => QuestionaryStatusEnum.Pending
        };
        return await GetByEnumAsync(statusEnum, ct);
    }
    public async Task AddAsync(QuestionaryStatuses status, CancellationToken ct)
    {
        await _context.QuestionnaireStatuses.AddAsync(status, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<Result<QuestionaryStatuses, QuestionnaireStatusException>> UpdateAsync(QuestionaryStatuses status, CancellationToken ct)
    {
        _context.QuestionnaireStatuses.Update(status);
        await _context.SaveChangesAsync(ct);
        
        return status;
    }

    public async Task<Result<QuestionaryStatuses, QuestionnaireStatusException>> DeleteAsync(QuestionaryStatuses status, CancellationToken ct)
    {
        _context.QuestionnaireStatuses.Remove(status);
        await _context.SaveChangesAsync(ct);
        
        return status;
    }

    public async Task<Option<QuestionaryStatuses>> GetByIdAsync(questionaryStatusId id, CancellationToken ct)
    {
        var entity = await _context.QuestionnaireStatuses
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        
        return entity == null ? Option.None<QuestionaryStatuses>() : Option.Some(entity);
    }

    public async Task<IEnumerable<QuestionaryStatuses>> GetAllAsync(CancellationToken ct)
    {
        return await _context
            .QuestionnaireStatuses
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public async Task<Option<QuestionaryStatuses>> GetByEnumAsync(QuestionaryStatusEnum status, CancellationToken ct)
    {
        var entity = await _context.QuestionnaireStatuses
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Status == status, ct);
        return entity == null ? Option.None<QuestionaryStatuses>() : Option.Some(entity);
    }
} 