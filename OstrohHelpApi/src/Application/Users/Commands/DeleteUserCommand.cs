using Application.Common;
using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using Application.Users.Exceptions;
using Domain.Users;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Users.Commands;

public record DeleteUserCommand : IRequest<Result<User, UserDeleteExceptions>>
{
    public required Guid UserId { get; init; }
}

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, Result<User, UserDeleteExceptions>>
{
    private readonly IUserRepository _userRepository;
    private readonly IUserQuery _userQuery;
    private readonly ILogger<DeleteUserCommandHandler> _logger;

    public DeleteUserCommandHandler(IUserRepository userRepository, IUserQuery userQuery, ILogger<DeleteUserCommandHandler> logger)
    {
        _userRepository = userRepository;
        _userQuery = userQuery;
        _logger = logger;
    }

    public async Task<Result<User, UserDeleteExceptions>> Handle(DeleteUserCommand request, CancellationToken ct)
    {
        var userId = new UserId(request.UserId);

        var existingUser = await _userQuery.GetByIdAsync(userId, ct);

        return await existingUser.Match(
            u => DeleteEntity(u, ct),
            () => Task.FromResult<Result<User, UserDeleteExceptions>>(new UserNotFoundExceptions(userId))
        );
    }

    public async Task<Result<User, UserDeleteExceptions>> DeleteEntity(User user, CancellationToken ct)
    {
        try
        {
            var deletedUser = await _userRepository.DeleteAsync(user, ct);
            return deletedUser;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while deleting a user with id {UserId}", user.Id);
            return new UserUnknownException(user.Id, e);
        }
    }
}