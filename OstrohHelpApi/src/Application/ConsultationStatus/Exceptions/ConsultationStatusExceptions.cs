using Domain.Conferences.Statuses;

namespace Application.ConsultationStatus.Exceptions;

public abstract class ConsultationStatusExceptions(Guid id, string message, Exception? inner = null)
    : Exception(message, inner)
{
    public Guid Id { get; } = id;
}

public class ConsultationStatusNotFoundException : ConsultationStatusExceptions
{
    public ConsultationStatusesId StatusId { get; }

    public ConsultationStatusNotFoundException(ConsultationStatusesId id)
        : base(id.Value, $"Consultation status with ID '{id}' not found.")
    {
        StatusId = id;
    }
}

public class InvalidConsultationStatusException : ConsultationStatusExceptions
{
    public ConsultationStatusesId StatusId { get; }

    public InvalidConsultationStatusException(ConsultationStatusesId id)
        : base(id.Value, $"Consultation status with ID '{id}' is invalid.")
    {
        StatusId = id;
    }
}

public class SomethingWrongWithConsultationStatus : ConsultationStatusExceptions
{
    public SomethingWrongWithConsultationStatus(ConsultationStatusesId id)
        : base(id.Value, $"Something wrong with consultation status with ID '{id}'.")
    {
    }
}
