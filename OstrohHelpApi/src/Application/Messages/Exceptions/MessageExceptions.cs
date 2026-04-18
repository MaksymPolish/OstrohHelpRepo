using Domain.Conferences;
using Domain.Messages;
using Domain.Users;

namespace Application.Messages.Exceptions;

public abstract class MessageExceptions : Exception
{
    protected MessageExceptions(string message) : base(message) { }
}

public class MessageUnknownException : MessageExceptions
{
    public MessageUnknownException(Exception inner)
        : base($"An unknown error occurred while sending the message. {inner.Message}")
    {
    }
}

public class MessageNotFoundException : MessageExceptions
{
    public Guid MessageId { get; }
    public MessageNotFoundException(Guid id)
        : base($"The message with '{id}' not found.")
    {
        MessageId = id;
    }
    
}

public class InvalidSenderIdException : MessageExceptions
{
    public Guid SenderId { get; }

    public InvalidSenderIdException(Guid id)
        : base($"User with ID '{id}' cannot send a message.")
    {
        SenderId = id;
    }
}

public class ConsultationNotFoundException : MessageExceptions
{
    public Guid ConsultationId { get; }

    public ConsultationNotFoundException(Guid id)
        : base($"The consultation with ID '{id}' was not found.")
    {
        ConsultationId = id;
    }
}