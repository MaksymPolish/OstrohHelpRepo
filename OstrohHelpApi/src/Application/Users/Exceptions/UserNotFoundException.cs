using Domain.Users;

namespace Application.Users.Exceptions;

public class UserNotFoundException : Exception
{
    public UserNotFoundException(Guid userId) 
        : base($"User with ID {userId} was not found.") { }

    public UserNotFoundException(string message) : base(message) { }

    public UserNotFoundException(UserId userId) : base($"User with ID {userId} was not found.") { }
}