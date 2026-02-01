using Domain.Conferences;
using Domain.Users;
using Optional;

namespace Application.Common.Interfaces.Queries;

public interface IConsultationQuery
{
    Task<IEnumerable<Domain.Conferences.Consultations>> GetAllAsync(CancellationToken ct);
    
    /// Отримати всі консультації з деталями (Status, Student, Psychologist) - вирішує N+1
    Task<IEnumerable<Domain.Conferences.Consultations>> GetAllWithDetailsAsync(CancellationToken ct);
    
    Task<Option<Domain.Conferences.Consultations>> GetByIdAsync(ConsultationsId id, CancellationToken ct);
    
    /// Отримати консультацію за ID з деталями - вирішує N+1
    Task<Option<Domain.Conferences.Consultations>> GetByIdWithDetailsAsync(ConsultationsId id, CancellationToken ct);
    
    Task<IEnumerable<Domain.Conferences.Consultations>> GetAllByUserIdAsync(UserId id, CancellationToken ct);
    
    /// Отримати всі консультації користувача з деталями - вирішує N+1
    Task<IEnumerable<Domain.Conferences.Consultations>> GetAllByUserIdWithDetailsAsync(UserId id, CancellationToken ct);
}