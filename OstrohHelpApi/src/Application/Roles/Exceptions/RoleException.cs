using Domain.Users.Roles;

namespace Application.Roles.Exceptions;

public class RoleException(Guid id, string message, Exception? exception = null) : Exception(message, exception)
{
    public Guid RoleId { get; } = id;
}

public class RoleNotFoundException : RoleException
{

    public RoleNotFoundException(Guid id)
        : base(id, $"Role with ID {id} was not found.")
    {
    }
}

public class RoleUnknownException : RoleException
{
    public RoleUnknownException(Guid id) 
        : base(id, $"An unknown error occurred while processing the role with ID: {id}")
    {
    }
}
