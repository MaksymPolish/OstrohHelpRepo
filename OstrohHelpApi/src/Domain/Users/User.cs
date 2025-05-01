using Domain.Users.Roles;

namespace Domain.Users;

public class User
{
    public UserId Id { get; set; }
    public RoleId RoleId { get; set; }
    public string GoogleId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string? Course { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public bool IsLoggedIn { get; set; }
    public string AuthToken { get; set; }
    public DateTime? TokenExpiration { get; set; }
    public DateTime CreatedAt { get; set; }
   
    
}