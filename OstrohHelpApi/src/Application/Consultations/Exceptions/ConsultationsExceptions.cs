using Application.Questionnaire.Exceptions;
using Domain.Conferences;
using Domain.Conferences.Statuses;
using Domain.Inventory;
using Domain.Inventory.Statuses;
using Domain.Users;
using Domain.Users.Roles;

namespace Application.Consultations.Exceptions;

public abstract class ConsultationsExceptions(Guid id, string message, Exception? inner = null)
    : Exception(message, inner)
{
    public Guid Id { get; } = id;
}

public class InvalidPsychologistRoleException : ConsultationsExceptions
{
    public Guid UserId { get; }

    public InvalidPsychologistRoleException(Guid id)
        : base(id, $"User with ID '{id}' is not a psychologist.")
    {
        UserId = id;
    }
}

public class ConsultationNotFoundException : ConsultationsExceptions
{
    public Guid ConsultationId { get; }

    public ConsultationNotFoundException(Guid id)
        : base(id, $"Consultation with ID '{id}' not found.")
    {
        ConsultationId = id;
    }
}

public class SometingWrongWithConsultation : ConsultationsExceptions
{
    public SometingWrongWithConsultation(Guid id)
        : base(id, $"Something wrong with consultation with ID '{id}'.")
    {
    }
}