using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using Application.Users.Exceptions;
using Domain.Users;
using MediatR;

namespace Application.Users.Commands;

public record AddUserCourseCommand : IRequest
{
    public required Guid UserId { get; init; }
    public required string Course { get; init; }
}

public class AddUserCourseCommandHandler(IUserRepository _userRepository, IUserQuery _userQuery) 
    : IRequestHandler<AddUserCourseCommand>
{
    public async Task Handle(AddUserCourseCommand request, CancellationToken ct)
    {
        var userOption = await _userQuery.GetByIdAsync(request.UserId, ct);

        await userOption.Match(
            async user =>
            {
                user.Course = request.Course;
                await _userRepository.UpdateAsync(user, ct);
            },
            () => throw new UserNotFoundException(request.UserId)
        );
    }
}