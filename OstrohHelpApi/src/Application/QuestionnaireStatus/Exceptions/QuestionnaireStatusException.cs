using Domain.Inventory.Statuses;

namespace Application.QuestionnaireStatus.Exceptions;

public abstract class QuestionnaireStatusException(Guid id, string message, Exception? inner = null)
    : Exception(message, inner)
{
    public Guid Id { get; } = id;
}

public class QuestionnaireStatusNotFoundException : QuestionnaireStatusException
{
    public Guid StatusId { get; }

    public QuestionnaireStatusNotFoundException(Guid id)
        : base(id, $"Questionnaire status with ID '{id}' not found.")
    {
        StatusId = id;
    }
}

public class InvalidQuestionnaireStatusException : QuestionnaireStatusException
{
    public Guid StatusId { get; }

    public InvalidQuestionnaireStatusException(Guid id)
        : base(id, $"Questionnaire status with ID '{id}' is invalid.")
    {
        StatusId = id;
    }
}

public class SomethingWrongWithQuestionnaireStatus : QuestionnaireStatusException
{
    public SomethingWrongWithQuestionnaireStatus(Guid id)
        : base(id, $"Something wrong with questionnaire status with ID '{id}'.")
    {
    }
}
