using Application.Common;
using Domain.Conferences.Statuses;
using MediatR;

namespace Application.ConsultationStatus.Exceptions;

public abstract class ConsultationStatusExceptions(ConsultationStatusesId id, 
    string message, 
    Exception? exception = null) 
    : Exception(message, exception), IRequest<Result<ConsultationStatuses, ConsultationStatusExceptions>>
{
    public ConsultationStatusesId Id { get; } = id;
}

public class ConsultationStatusUnknownException : ConsultationStatusExceptions
{
    public ConsultationStatusUnknownException(ConsultationStatusesId id, Exception innerException) 
        : base(id, $"An unknown error occurred while processing consultation status '{id}'.", innerException) { }
}

public class ConsultationStatusNotFoundExceptions : ConsultationStatusExceptions
{
    public ConsultationStatusNotFoundExceptions(ConsultationStatusesId id) 
        : base(id, $"Consultation status with ID '{id}' was not found.") { }
}

