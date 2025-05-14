using Domain.Inventory.Statuses;

namespace Application.QuestionnaireStatus.Exceptions;

public abstract class QuestionnaireStatusException(questionaryStatusId id, string message, Exception? exception = null) : Exception(message, exception)
{
    public questionaryStatusId StatusId { get; } = id;
}

public class QuestionaryNotFoundException : QuestionnaireStatusException
{
    public QuestionaryNotFoundException(questionaryStatusId id)
        : base(id, $"Questionary status with ID '{id}' was not found.")
    {
    }
}

public class QuestionaryUnknownException : QuestionnaireStatusException
{
    public questionaryStatusId StatusId { get; }

    public QuestionaryUnknownException(questionaryStatusId id, Exception inner)
        : base(id, $"An unknown error occurred while processing questionary status '{id}'.", inner)
    {
    }
}
