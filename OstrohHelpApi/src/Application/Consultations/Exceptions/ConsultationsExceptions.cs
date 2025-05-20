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
    public UserId UserId { get; }

    public InvalidPsychologistRoleException(UserId id)
        : base(id.Value, $"User with ID '{id}' is not a psychologist.")
    {
        UserId = id;
    }
}

public class ConsultationNotFoundException : ConsultationsExceptions
{
    public ConsultationsId ConsultationId { get; }

    public ConsultationNotFoundException(ConsultationsId id)
        : base(id.Value, $"Consultation with ID '{id}' not found.")
    {
        ConsultationId = id;
    }
}

public class SometingWrongWithConsultation : ConsultationsExceptions
{
    public SometingWrongWithConsultation(ConsultationsId id)
        : base(id.Value, $"Something wrong with consultation with ID '{id}'.")
    {
    }
}