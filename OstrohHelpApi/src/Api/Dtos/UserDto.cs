using Domain.Users;
using Domain.Users.Roles;

namespace Api.Dtos;

public record UserDto(
    Guid Id,
    string Email,
    string FullName,
    Guid RoleId,
    string GoogleId
    
    )
{
    public static UserDto FromDomainModel(User user) 
        => new(
            Id: user.Id.Value,
            Email: user.Email,
            FullName: user.FullName,
            RoleId: user.RoleId.Value,
            GoogleId: user.GoogleId
            );
    public string GoogleId { get; set; }
    
    public bool IsAuthenticated => !string.IsNullOrEmpty(GoogleId);
}