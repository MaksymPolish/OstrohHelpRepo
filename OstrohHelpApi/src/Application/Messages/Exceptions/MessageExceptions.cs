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
    public MessageId MessageId { get; }
    public MessageNotFoundException(MessageId id)
        : base($"The message with '{id}' not found.")
    {
        MessageId = id;
    }
    
}

public class InvalidSenderIdException : MessageExceptions
{
    public UserId SenderId { get; }

    public InvalidSenderIdException(UserId id)
        : base($"User with ID '{id}' cannot send a message.")
    {
        SenderId = id;
    }
}

public class ConsultationNotFoundException : MessageExceptions
{
    public ConsultationsId ConsultationId { get; }

    public ConsultationNotFoundException(ConsultationsId id)
        : base($"The consultation with ID '{id}' was not found.")
    {
        ConsultationId = id;
    }
}