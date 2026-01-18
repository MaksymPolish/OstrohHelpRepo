using Application.Common;
using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using Application.ConsultationStatus.Exceptions;
using Domain.Conferences.Statuses;
using Microsoft.EntityFrameworkCore;
using Optional;

namespace Infrastructure.Persistence.Repositories;

public class ConsultationStatusRepository : IConsultationStatusQuery, IConsultationStatusRepository
{
    // Mapping between enum and Guid
    private static readonly Dictionary<ConsultationStatusEnum, Guid> StatusEnumToGuid = new()
    {
        { ConsultationStatusEnum.Assigned, new Guid("00000000-0000-0000-0000-000000000011") },
        { ConsultationStatusEnum.Rejected, new Guid("00000000-0000-0000-0000-000000000012") },
        { ConsultationStatusEnum.Pending, new Guid("00000000-0000-0000-0000-000000000013") },
        // Додайте інші статуси за потреби
    };

    private static readonly Dictionary<Guid, ConsultationStatusEnum> GuidToStatusEnum = StatusEnumToGuid.ToDictionary(kv => kv.Value, kv => kv.Key);

    public static Guid GetGuidByEnum(ConsultationStatusEnum statusEnum)
    {
        return StatusEnumToGuid.TryGetValue(statusEnum, out var guid) ? guid : Guid.Empty;
    }

    public static ConsultationStatusEnum GetEnumByGuid(Guid guid)
    {
        return GuidToStatusEnum.TryGetValue(guid, out var statusEnum) ? statusEnum : ConsultationStatusEnum.Pending;
    }
    private readonly ApplicationDbContext context;
    public ConsultationStatusRepository(ApplicationDbContext context)
    {
        this.context = context;
    }

    [Obsolete("Use GetByEnumAsync instead")]
    public async Task<Option<ConsultationStatuses>> GetByNameAsync(string name, CancellationToken cancellationToken)
    {
        // Map name to enum
        var statusEnum = name switch
        {
            "Назначено" => ConsultationStatusEnum.Assigned,
            "Відхилено" => ConsultationStatusEnum.Rejected,
            "Завершено" => ConsultationStatusEnum.Completed,
            "Очікує підтвердження" => ConsultationStatusEnum.Pending,
            _ => ConsultationStatusEnum.Pending
        };
        return await GetByEnumAsync(statusEnum, cancellationToken);
    }
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

    public async Task<Option<ConsultationStatuses>> GetByEnumAsync(ConsultationStatusEnum status, CancellationToken cancellationToken)
    {
        var guid = GetGuidByEnum(status);
        var entity = await context.ConsultationStatuses
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id.Value == guid, cancellationToken);
        return entity == null ? Option.None<ConsultationStatuses>() : Option.Some(entity);
    }
    
    
}