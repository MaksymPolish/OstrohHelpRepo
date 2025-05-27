namespace Application.Users.Exceptions;

public class InvalidFirebaseTokenException : Exception
{
    public InvalidFirebaseTokenException() : base("Failed to validate Firebase ID token.") { }
}