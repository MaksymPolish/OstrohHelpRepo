using Application.Common;
using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using Application.QuestionnaireStatus.Exceptions;
using Domain.Questionnaires;
using Domain.Questionnaires.Statuses;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Optional;

namespace Infrastructure.Persistence.Repositories;

public class QuestionnaireStatusRepository(ApplicationDbContext _context) : IQuestionnaireStatusRepository, IQuestionnaireStatusQuery
{
    public async Task AddAsync(QuestionnaireStatuses status, CancellationToken ct)
    {
        await _context.QuestionnaireStatuses.AddAsync(status, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<Result<QuestionnaireStatuses, QuestionnaireException>> UpdateAsync(QuestionnaireStatuses status, CancellationToken ct)
    {
        _context.QuestionnaireStatuses.Update(status);
        await _context.SaveChangesAsync(ct);
        
        return status;
    }

    public async Task<Result<QuestionnaireStatuses, QuestionnaireException>> DeleteAsync(QuestionnaireStatuses status, CancellationToken ct)
    {
        _context.QuestionnaireStatuses.Remove(status);
        await _context.SaveChangesAsync(ct);
        
        return status;
    }

    public async Task<Option<QuestionnaireStatuses>> GetByIdAsync(QuestionnaireStatusesId id, CancellationToken ct)
    {
        var entity = await _context.QuestionnaireStatuses
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        
        return entity == null ? Option.None<QuestionnaireStatuses>() : Option.Some(entity);
    }

    public async Task<IEnumerable<QuestionnaireStatuses>> GetAllAsync(CancellationToken ct)
    {
        return await _context
            .QuestionnaireStatuses
            .AsNoTracking()
            .ToListAsync(ct);
    }
} 