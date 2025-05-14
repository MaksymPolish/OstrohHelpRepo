using Application.Common;
using Domain.Inventory;
using MediatR;

namespace Application.Questionnaire.Exceptions;

public abstract class QuestionnairesException(QuestionaryId id, string message, Exception? exception = null) : Exception(message, exception), IRequest<Result<Questionary, QuestionnairesException>>
{
    public QuestionaryId questionaryId { get; } = id;
}

public class QuestionnaireUnknownException : QuestionnairesException
{
    public QuestionnaireUnknownException(QuestionaryId id, Exception innerException) : base(id, $"An unknown error occurred while processing questionnaires '{id}'.", innerException) { }
}

public class QuestionnaireNotFoundException : QuestionnairesException
{
    public QuestionnaireNotFoundException(QuestionaryId id) : base(id, $"Questionnaire with ID '{id}' was not found.") { }
}