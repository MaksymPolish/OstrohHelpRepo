using Application.Common.Interfaces.Queries;
using Domain.Conferences;
using Domain.Messages;
using Domain.Users;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

/// <summary>
/// Реалізація перевірки доступу користувача до консультацій та месседжів
/// Забезпечує правильну авторизацію на рівні даних
/// </summary>
public class ConsultationAccessChecker(ApplicationDbContext context) : IConsultationAccessChecker
{
    /// <summary>
    /// Перевіряє, чи користувач є учасником консультації
    /// </summary>
    public async Task<bool> IsConsultationMember(Guid userId, Guid consultationId, CancellationToken ct)
    {
        var consultation = await context.Consultations
            .AsNoTracking()
            .FirstOrDefaultAsync(
                c => c.Id == consultationId &&
                     (c.StudentId == userId || c.PsychologistId == userId),
                ct);

        return consultation != null;
    }

    /// <summary>
    /// Перевіряє, чи користувач є власником повідомлення
    /// </summary>
    public async Task<bool> IsMessageOwner(Guid userId, Guid messageId, CancellationToken ct)
    {
        var message = await context.Messages
            .AsNoTracking()
            .FirstOrDefaultAsync(
                m => m.Id == messageId &&
                     m.SenderId == userId,
                ct);

        return message != null;
    }

    /// <summary>
    /// Отримує роль користувача в консультації
    /// </summary>
    public async Task<ConsultationRole?> GetUserRoleInConsultation(Guid userId, Guid consultationId, CancellationToken ct)
    {
        var consultation = await context.Consultations
            .AsNoTracking()
            .FirstOrDefaultAsync(
                c => c.Id == consultationId,
                ct);

        if (consultation == null)
            return null;

        if (consultation.StudentId == userId)
            return ConsultationRole.Student;

        if (consultation.PsychologistId == userId)
            return ConsultationRole.Psychologist;

        return null;
    }

    /// <summary>
    /// Перевіряє, чи користувач може читати повідомлення з цієї консультації
    /// </summary>
    public async Task<bool> CanViewConsultationMessages(Guid userId, Guid consultationId, CancellationToken ct)
    {
        // Користувач може читати тільки коли він член консультації
        return await IsConsultationMember(userId, consultationId, ct);
    }

    /// <summary>
    /// Отримує ID консультації для повідомлення
    /// </summary>
    public async Task<Guid?> GetConsultationIdForMessage(Guid messageId, CancellationToken ct)
    {
        var message = await context.Messages
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == messageId, ct);

        return message?.ConsultationId;
    }
}
