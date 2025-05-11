using Application.Common;
using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using Application.Roles.Exceptions;
using Application.Users.Exceptions;
using Domain.Users.Roles;
using MediatR;

namespace Application.Roles.Commands;

public record DeleteRoleCommand : IRequest<Result<Role, RoleException>>
{
    public required Guid Id { get; init; }
}

public class DeleteRoleCommandHandler(IRoleRepository _roleRepository, IRoleQuery _roleQuery)
    : IRequestHandler<DeleteRoleCommand, Result<Role, RoleException>>
{
    public async Task<Result<Role, RoleException>> Handle(DeleteRoleCommand command, CancellationToken ct)
    {
        var roleId = new RoleId(command.Id);

        var role = await _roleQuery.GetByIdAsync(roleId, ct);
        
        return await role.Match(
            async r =>
            {
                return await DeleteEntity(r, ct);
            },
            () => Task.FromResult<Result<Role, RoleException>>(new RoleUnknownException(roleId)
        ));
    }

    public async Task<Result<Role, RoleException>> DeleteEntity(Role role, CancellationToken ct)
    {
        try
        {
            var deleteEntity = await _roleRepository.DeleteAsync(role, ct);

            return deleteEntity;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}