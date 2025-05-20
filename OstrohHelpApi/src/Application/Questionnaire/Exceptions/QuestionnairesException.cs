using Application.Common;
using Domain.Conferences.Statuses;
using Domain.Inventory;
using Domain.Inventory.Statuses;
using Domain.Users;
using Domain.Users.Roles;
using MediatR;

namespace Application.Questionnaire.Exceptions;

public abstract class QuestionnairesException(QuestionaryId id, string message, Exception? exception = null) 
    : Exception(message, exception), IRequest<Result<Questionary, QuestionnairesException>>
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

public class QuestionnaireAlreadyAcceptedException : QuestionnairesException
{
    public QuestionaryId Id { get; }

    public QuestionnaireAlreadyAcceptedException(QuestionaryId id)
        : base(id, $"This questionary is already accepted. ID: {id}")
    {
        Id = id;
    }
}

