using Domain.Inventory.Statuses;

namespace Application.QuestionnaireStatus.Exceptions;

public abstract class QuestionnaireStatusException(Guid id, string message, Exception? inner = null)
    : Exception(message, inner)
{
    public Guid Id { get; } = id;
}

public class QuestionnaireStatusNotFoundException : QuestionnaireStatusException
{
    public questionaryStatusId StatusId { get; }

    public QuestionnaireStatusNotFoundException(questionaryStatusId id)
        : base(id.Value, $"Questionnaire status with ID '{id}' not found.")
    {
        StatusId = id;
    }
}

public class InvalidQuestionnaireStatusException : QuestionnaireStatusException
{
    public questionaryStatusId StatusId { get; }

    public InvalidQuestionnaireStatusException(questionaryStatusId id)
        : base(id.Value, $"Questionnaire status with ID '{id}' is invalid.")
    {
        StatusId = id;
    }
}

public class SomethingWrongWithQuestionnaireStatus : QuestionnaireStatusException
{
    public SomethingWrongWithQuestionnaireStatus(questionaryStatusId id)
        : base(id.Value, $"Something wrong with questionnaire status with ID '{id}'.")
    {
    }
}
