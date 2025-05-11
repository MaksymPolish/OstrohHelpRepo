using Domain.Users;

namespace Application.Users.Exceptions;

public class UserDeleteExceptions(UserId id, string message, Exception? innerException = null)
    : Exception(message, innerException)
{
    public UserId UserId { get; } = id;
}

public class UserNotFoundExceptions : UserDeleteExceptions
{
    public UserNotFoundExceptions(UserId id) 
        : base(id, $"User with ID {id} was not found.")
    {
    }
}

public class UserUnknownException : UserDeleteExceptions
{
    public UserUnknownException(UserId id, Exception innerException)
        : base(id, $"An unknown error occurred while deleting user {id}.", innerException)
    {
    }
}
