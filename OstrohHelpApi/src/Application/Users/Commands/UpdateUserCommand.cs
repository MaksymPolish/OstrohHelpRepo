using Application.Common;
using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using Application.Users.Exceptions;
using Domain.Users;
using Domain.Users.Roles;
using MediatR;

namespace Application.Users.Commands;

public record UpdateUserCommand(Guid userId, Guid roleId) : IRequest<User>;

public class UpdateUserCommandHandler(IUserRepository _userRepository, IUserQuery _userQuery) : IRequestHandler<UpdateUserCommand, User>
{
    public async Task<User> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var userId = new UserId(request.userId);
        var roleId = new RoleId(request.roleId);

        var userOption = await _userQuery.GetByIdAsync(userId, cancellationToken);

        return await userOption.Match(
            async user =>
            {
                user.RoleId = roleId;
                await _userRepository.UpdateAsync(user, cancellationToken);
                return user;
            },
            () => throw new UserNotFoundException(userId)
        );
    }
}
