using Application.QuestionnaireStatus.Exceptions;
using Domain.Questionnaires.Statuses;
using MediatR;

namespace Application.Common.Interfaces.Repositories;

public interface IQuestionnaireStatusRepository
{
    Task AddAsync(QuestionnaireStatuses status, CancellationToken ct);
    Task<Result<QuestionnaireStatuses, QuestionnaireException>> UpdateAsync(QuestionnaireStatuses status, CancellationToken ct);
    Task<Result<QuestionnaireStatuses, QuestionnaireException>> DeleteAsync(QuestionnaireStatuses status, CancellationToken ct);
}