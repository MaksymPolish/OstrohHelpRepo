using Application.QuestionnaireStatus.Exceptions;
using Domain.Inventory.Statuses;

namespace Application.Common.Interfaces.Repositories;

public interface IQuestionnaireStatusRepository
{
    Task AddAsync(QuestionaryStatuses status, CancellationToken ct);
    Task<Result<QuestionaryStatuses, QuestionnaireStatusException>> UpdateAsync(QuestionaryStatuses status, CancellationToken ct);
    Task<Result<QuestionaryStatuses, QuestionnaireStatusException>> DeleteAsync(QuestionaryStatuses status, CancellationToken ct);
}