using Domain.Questionnaires;
using Domain.Questionnaires.Statuses;
using Optional;

namespace Application.Common.Interfaces.Queries;

public interface IQuestionnaireStatusQuery
{
    Task<Option<QuestionnaireStatuses>> GetByIdAsync(QuestionnaireStatusesId id, CancellationToken ct);
    Task<IEnumerable<QuestionnaireStatuses>> GetAllAsync(CancellationToken ct);
}