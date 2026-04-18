using Domain.Conferences.Statuses;

namespace Application.ConsultationStatus.Exceptions;

public abstract class ConsultationStatusExceptions(Guid id, string message, Exception? inner = null)
    : Exception(message, inner)
{
    public Guid Id { get; } = id;
}

public class ConsultationStatusNotFoundException : ConsultationStatusExceptions
{
    public Guid StatusId { get; }

    public ConsultationStatusNotFoundException(Guid id)
        : base(id, $"Consultation status with ID '{id}' not found.")
    {
        StatusId = id;
    }
}

public class InvalidConsultationStatusException : ConsultationStatusExceptions
{
    public Guid StatusId { get; }

    public InvalidConsultationStatusException(Guid id)
        : base(id, $"Consultation status with ID '{id}' is invalid.")
    {
        StatusId = id;
    }
}

public class SomethingWrongWithConsultationStatus : ConsultationStatusExceptions
{
    public SomethingWrongWithConsultationStatus(Guid id)
        : base(id, $"Something wrong with consultation status with ID '{id}'.")
    {
    }
}
