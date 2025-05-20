using Application.Common;
using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using Application.ConsultationStatus.Exceptions;
using Domain.Conferences.Statuses;
using Microsoft.EntityFrameworkCore;
using Optional;

namespace Infrastructure.Persistence.Repositories;

public class ConsultationStatusRepository(ApplicationDbContext context) : IConsultationStatusQuery, IConsultationStatusRepository
{
    public async Task AddAsync(ConsultationStatuses status, CancellationToken ct)
    {
        await context.ConsultationStatuses.AddAsync(status, ct);
        await context.SaveChangesAsync(ct);
    }

    public Task<Result<ConsultationStatuses, ConsultationStatusExceptions>> UpdateAsync(ConsultationStatuses status, CancellationToken ct)
    {
        context.ConsultationStatuses.Update(status);
        context.SaveChanges();
        
        return Task.FromResult<Result<ConsultationStatuses, ConsultationStatusExceptions>>(status);
    }

    public Task<Result<ConsultationStatuses, ConsultationStatusExceptions>> DeleteAsync(ConsultationStatuses status, CancellationToken ct)
    {
        context.ConsultationStatuses.Remove(status);
        context.SaveChanges();
        
        return Task.FromResult<Result<ConsultationStatuses, ConsultationStatusExceptions>>(status);
    }

    public async Task<IEnumerable<ConsultationStatuses>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await context.ConsultationStatuses.ToListAsync(cancellationToken);   
    }

    public async Task<Option<ConsultationStatuses>> GetByIdAsync(ConsultationStatusesId id, CancellationToken cancellationToken)
    {
        var entity = await context.ConsultationStatuses
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return entity == null ? Option.None<ConsultationStatuses>() : Option.Some(entity);
    }

    public async Task<Option<ConsultationStatuses>> GetByNameAsync(string name, CancellationToken cancellationToken)
    {
        var entity = await context.ConsultationStatuses
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Name == name, cancellationToken);
        
        return entity == null ? Option.None<ConsultationStatuses>() : Option.Some(entity);
    }
    
    
}