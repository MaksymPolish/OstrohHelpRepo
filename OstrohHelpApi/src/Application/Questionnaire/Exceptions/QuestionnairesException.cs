using Application.Common;
using Domain.Conferences.Statuses;
using Domain.Inventory;
using Domain.Inventory.Statuses;
using Domain.Users;
using Domain.Users.Roles;
using MediatR;

namespace Application.Questionnaire.Exceptions;

public abstract class QuestionnairesException : Exception
{
    protected QuestionnairesException(string message, Exception? inner = null) : base(message, inner) { }
}

public class QuestionnaireUnknownException : QuestionnairesException
{
    public QuestionnaireUnknownException(QuestionaryId id, Exception inner) 
        : base($"An unknown error occurred while processing questionnaires with ID '{id}'.", inner)
    {
    }
}

public class QuestionnaireNotFoundException : QuestionnairesException
{
    public QuestionnaireNotFoundException(QuestionaryId id) : base($"Questionnaire with ID '{id}' was not found.") { }
}

public class QuestionnaireAlreadyAcceptedException : QuestionnairesException
{
    public QuestionnaireAlreadyAcceptedException(QuestionaryId id)
        : base($"This questionary is already accepted. ID: {id}")
    {
    }
}

public class QuestionnaireStatusNotFoundException : QuestionnairesException
{
    public QuestionnaireStatusNotFoundException(string name) 
        : base($"Questionnaire status '{name}' not found.")
    {
    }
}

