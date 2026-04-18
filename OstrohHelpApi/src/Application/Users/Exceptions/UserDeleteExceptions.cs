using Domain.Users;

namespace Application.Users.Exceptions;

public class UserDeleteExceptions(Guid id, string message, Exception? innerException = null)
    : Exception(message, innerException)
{
    public Guid UserId { get; } = id;
}

public class UserNotFoundExceptions : UserDeleteExceptions
{
    public UserNotFoundExceptions(Guid id) 
        : base(id, $"User with ID {id} was not found.")
    {
    }
}

public class UserUnknownException : UserDeleteExceptions
{
    public UserUnknownException(Guid id, Exception innerException)
        : base(id, $"An unknown error occurred while deleting user {id}.", innerException)
    {
    }
}
