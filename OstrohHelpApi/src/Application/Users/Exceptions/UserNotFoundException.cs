namespace Application.Users.Exceptions;

public class UserNotFoundException : Exception
{
    public UserNotFoundException(int userId) 
        : base($"User with ID {userId} was not found.") { }

    public UserNotFoundException(string message) : base(message) { }
}