﻿using Application.Common;
using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using Application.Consultations.Exceptions;
using Domain.Conferences;
using Domain.Users;
using Google.Apis.Util;
using Microsoft.EntityFrameworkCore;
using Optional;

namespace Infrastructure.Persistence.Repositories;

public class ConsultationRepository(ApplicationDbContext context) : IConsultationRepository, IConsultationQuery
{
    public async Task<Result<Consultations, ConsultationsExceptions>> AddAsync(Consultations consultation, CancellationToken cancellationToken)
    {
        await context.Consultations.AddAsync(consultation, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return consultation;
    }

    public async Task<Result<Consultations, ConsultationsExceptions>> UpdateAsync(Consultations consultation, CancellationToken cancellationToken)
    {
        context.Consultations.Update(consultation);
        await context.SaveChangesAsync(cancellationToken);
        return consultation;
    }

    public async Task<Result<Consultations, ConsultationsExceptions>> DeleteAsync(Consultations consultation, CancellationToken cancellationToken)
    {
        context.Consultations.Remove(consultation);
        await context.SaveChangesAsync(cancellationToken);
        return consultation; 
    }

    public async Task<IEnumerable<Consultations>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await context.Consultations.ToListAsync(cancellationToken);
    }

    public async Task<Option<Consultations>> GetByIdAsync(ConsultationsId id, CancellationToken ct)
    {
        var result = await context.Consultations.FirstOrDefaultAsync(x => x.Id == id, ct);
        return result != null ? Option.Some(result) : Option.None<Consultations>();
    }

    public async Task<IEnumerable<Consultations>> GetAllByUserIdAsync(UserId id, CancellationToken ct)
    {
        List<Consultations> resultList = await context.Consultations
            .AsNoTracking()
            .Where(x => x.PsychologistId == id || x.StudentId == id)
            .Distinct() // Уникаємо дублікатів
            .ToListAsync(ct);

        return resultList;
    }
}