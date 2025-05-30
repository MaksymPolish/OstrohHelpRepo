﻿using Application.Common;
using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using Application.Roles.Exceptions;
using Domain.Users.Roles;
using MediatR;

namespace Application.Roles.Commands;

public record UpdateRoleCommand : IRequest<Result<Role, RoleException>>
{
    public Guid Id { get; init; }
    public string Name { get; init; }
}

public class UpdateRoleCommandHandler(IRoleRepository _roleRepository, IRoleQuery _roleQuery) 
    : IRequestHandler<UpdateRoleCommand, Result<Role, RoleException>>
{
    public async Task<Result<Role, RoleException>> Handle(UpdateRoleCommand request, CancellationToken ct)
    {
        var roleId = new RoleId(request.Id);
        
        var role = await _roleQuery.GetByIdAsync(roleId, ct);

        return await role.Match(
            async r =>
            {
                r.Name = request.Name;
                return await UpdateEntity(r, ct);
            },
            () => Task.FromResult<Result<Role, RoleException>>(new RoleNotFoundException(roleId))
        );
    }

    public async Task<Result<Role, RoleException>> UpdateEntity(Role role, CancellationToken ct)
    {
        try
        {
            var updatedRole = await _roleRepository.UpdateAsync(role, ct);
            
            return updatedRole; 
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}