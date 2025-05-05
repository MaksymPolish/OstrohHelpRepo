using Domain.Users.Roles;

namespace Api.Dtos;

public class RoleDto
{
    public RoleId Id { get; set; }
    public string Name { get; set; }
}