using Domain.Users.Roles;

namespace Application.Roles.Exceptions;

public class RoleException(RoleId id, string message, Exception? exception = null) : Exception(message, exception)
{
    public RoleId RoleId { get; } = id;
}

public class RoleNotFoundException : RoleException
{

    public RoleNotFoundException(RoleId id)
        : base(id, $"Role with ID {id} was not found.")
    {
    }
}

public class RoleUnknownException : RoleException
{
    public RoleUnknownException(RoleId id) 
        : base(id, $"An unknown error occurred while processing the role with ID: {id}")
    {
    }
}
