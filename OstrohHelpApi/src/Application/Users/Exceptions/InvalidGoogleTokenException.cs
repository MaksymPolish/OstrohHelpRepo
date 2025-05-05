namespace Application.Users.Exceptions;

public class InvalidGoogleTokenException : Exception
{
    public InvalidGoogleTokenException() 
        : base("Provided Google token is invalid or expired.") { }

    public InvalidGoogleTokenException(string message) : base(message) { }
}