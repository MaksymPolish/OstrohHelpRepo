using Domain.Conferences;
using Domain.Users;

namespace Application.Common.Interfaces.Queries;

/// Виконує перевірку доступу користувача до консультацій та месседжів
public interface IConsultationAccessChecker
{
    /// <summary>
    /// Перевіряє, чи користувач є учасником консультації (студент або психолог)
    /// </summary>
    Task<bool> IsConsultationMember(Guid userId, Guid consultationId, CancellationToken ct);

    /// <summary>
    /// Перевіряє, чи користувач є власником (автором) повідомлення
    /// </summary>
    Task<bool> IsMessageOwner(Guid userId, Guid messageId, CancellationToken ct);

    /// <summary>
    /// Отримує роль користувача в консультації
    /// </summary>
    /// <returns>ConsultationRole.Student, .Psychologist, або null якщо користувач не член</returns>
    Task<ConsultationRole?> GetUserRoleInConsultation(Guid userId, Guid consultationId, CancellationToken ct);

    /// <summary>
    /// Перевіряє, чи користувач може читати повідомлення з цієї консультації
    /// (тільки члени консультації можуть читати)
    /// </summary>
    Task<bool> CanViewConsultationMessages(Guid userId, Guid consultationId, CancellationToken ct);

    /// <summary>
    /// Отримує ID консультації для повідомлення
    /// </summary>
    Task<Guid?> GetConsultationIdForMessage(Guid messageId, CancellationToken ct);
}

/// Ролі користувача в консультації
public enum ConsultationRole
{
    Student = 1,
    Psychologist = 2
}
