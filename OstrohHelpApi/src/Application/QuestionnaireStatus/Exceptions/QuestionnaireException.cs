using Domain.Questionnaires;

namespace Application.QuestionnaireStatus.Exceptions;

public abstract class QuestionnaireException(QuestionnaireStatusesId id, string message, Exception? exception = null) : Exception(message, exception)
{
    public QuestionnaireStatusesId StatusId { get; } = id;
}

public class QuestionaryNotFoundException : QuestionnaireException
{
    public QuestionaryNotFoundException(QuestionnaireStatusesId id)
        : base(id, $"Questionary status with ID '{id}' was not found.")
    {
    }
}

public class QuestionaryUnknownException : QuestionnaireException
{
    public QuestionnaireStatusesId StatusId { get; }

    public QuestionaryUnknownException(QuestionnaireStatusesId id, Exception inner)
        : base(id, $"An unknown error occurred while processing questionary status '{id}'.", inner)
    {
    }
}
