using Domain.Users;
using Domain.Users.Roles;

namespace Api.Dtos;

public class UserDto
{
    public UserId Id { get; set; }
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public RoleId RoleId { get; set; }
    public string GoogleId { get; set; }
    
    public bool IsAuthenticated => !string.IsNullOrEmpty(GoogleId);
}