using Application.Common.Interfaces.Repositories;
using Domain.Users.Roles;
using MediatR;

namespace Application.Roles.Commands;
public record CreateRoleCommand(string Name) : IRequest<Role>;

public class CreateRoleCommandHandler(IRoleRepository _roleRepository) : IRequestHandler<CreateRoleCommand, Role>
{

    public async Task<Role> Handle(CreateRoleCommand request, CancellationToken ct)
    {
        var roleId = RoleId.New(); 

        var role = Role.Create(roleId, request.Name);

        await _roleRepository.AddAsync(role, ct);

        return role;
    }
}