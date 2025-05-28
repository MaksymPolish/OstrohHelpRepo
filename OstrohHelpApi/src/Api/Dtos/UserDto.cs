using Domain.Users;
using Domain.Users.Roles;

namespace Api.Dtos;

// public record UserDto(
//     Guid Id,
//     string Email,
//     string FullName,
//     Guid RoleId,
//     string GoogleId,
//     string RoleName
//     )
// {
//     public static UserDto FromDomainModel(User user) 
//         => new(
//             Id: user.Id.Value,
//             Email: user.Email,
//             FullName: user.FullName,
//             RoleId: user.RoleId.Value,
//             GoogleId: user.GoogleId
//             );
//     public string GoogleId { get; set; }
//     
//     public bool IsAuthenticated => !string.IsNullOrEmpty(GoogleId);
// }

public class UserDto
{
    public Guid Id { get; set; }
    public string Email { get; set; }
    public string FullName { get; set; }
    public Guid RoleId { get; set; }
    public string GoogleId { get; set; }
    public string RoleName { get; set; }
    
    public string? Course { get; set; }
} 