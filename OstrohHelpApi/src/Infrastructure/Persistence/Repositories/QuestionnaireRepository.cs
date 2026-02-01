using Application.Common;
using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using Application.Questionnaire.Exceptions;
using Domain.Inventory;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using Optional;

namespace Infrastructure.Persistence.Repositories;

public class QuestionnaireRepository(ApplicationDbContext _context) : IQuestionnaireQuery, IQuestionnaireRepository
{
    public async Task AddAsync(Questionary questionary, CancellationToken ct)
    {
        await _context.Questionnaires.AddAsync(questionary, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<Result<Questionary, QuestionnairesException>> UpdateAsync(Questionary questionary, CancellationToken ct)
    {
        _context.Questionnaires.Update(questionary);
        await _context.SaveChangesAsync(ct);
        
        return questionary;
    }

    public Task<Result<Questionary, QuestionnairesException>> UpdateStatusAsync(Questionary questionary, CancellationToken ct)
    {
        _context.Questionnaires.Update(questionary);
        _context.SaveChanges();
        
        return Task.FromResult<Result<Questionary, QuestionnairesException>>(questionary);
    }

    public async Task<Questionary> DeleteAsync(Questionary questionary, CancellationToken ct)
    {
        _context.Questionnaires.Remove(questionary);
        await _context.SaveChangesAsync(ct);
        return questionary;
    }

    public async Task<IEnumerable<Questionary>> GetAllAsync(CancellationToken ct)
    {
        return await _context.Questionnaires.ToListAsync(ct);   
    }

    /// Отримати всі анкети з деталями (1 запит замість N+1)
    public async Task<IEnumerable<Questionary>> GetAllWithDetailsAsync(CancellationToken ct)
    {
        return await _context.Questionnaires
            .AsNoTracking()
            .Include(q => q.User)
            .Include(q => q.Status)
            .ToListAsync(ct);
    }

    public async Task<Option<Questionary>> GetByIdAsync(QuestionaryId id, CancellationToken ct)
    {
        var entity = await _context.Questionnaires
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        return entity == null ? Option.None<Questionary>() : Option.Some(entity);
    }

    /// Отримати анкету за ID з деталями (1 запит замість N+1)
    public async Task<Option<Questionary>> GetByIdWithDetailsAsync(QuestionaryId id, CancellationToken ct)
    {
        var entity = await _context.Questionnaires
            .AsNoTracking()
            .Include(q => q.User)
            .Include(q => q.Status)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        return entity == null ? Option.None<Questionary>() : Option.Some(entity);
    }

    public async Task<Option<Questionary>> GetByUserIdAsync(UserId id, CancellationToken ct)
    {
        var entity = await _context.Questionnaires
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == id, ct);

        return entity == null ? Option.None<Questionary>() : Option.Some(entity);
    }

    public async Task<IEnumerable<Questionary>> GetAllByUserIdAsync(UserId id, CancellationToken ct)
    {
        List<Questionary> questionaries = await _context.Questionnaires
            .AsNoTracking()
            .Where(x => x.UserId == id)
            .ToListAsync(ct);

        return questionaries;
    }

    /// Отримати всі анкети користувача з деталями (1 запит замість N+1)
    public async Task<IEnumerable<Questionary>> GetAllByUserIdWithDetailsAsync(UserId id, CancellationToken ct)
    {
        return await _context.Questionnaires
            .AsNoTracking()
            .Include(q => q.User)
            .Include(q => q.Status)
            .Where(x => x.UserId == id)
            .ToListAsync(ct);
    }
}