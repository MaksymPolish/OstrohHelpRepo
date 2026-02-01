using Domain.Users.Roles;
using Domain.Users.Tockens;

namespace Domain.Users;

public class User
{
    public UserId Id { get; set; }
    public RoleId RoleId { get; set; }
    
    // Навігаційна властивість для Role (вирішує N+1 проблему)
    public Role? Role { get; set; }
    
    public string GoogleId { get; set; }
    public string FullName { get; set; }
    public string? Course { get; set; }
    public string Email { get; set; }
    public bool IsLoggedIn { get; set; }
    public DateTime CreatedAt { get; set; }
    
    //Звязок з токенами
    private readonly List<UserToken> _tokens = new();
    public IReadOnlyCollection<UserToken> Tokens => _tokens.AsReadOnly();
   
    public User() => CreatedAt = DateTime.UtcNow;

    User(UserId userId, RoleId roleId, string googleId, string fullName, string course, string email, 
        bool isLoggedIn)
    {
        Id = userId;
        RoleId = roleId;
        GoogleId = googleId;
        FullName = fullName;
        Course = course;
        _tokens = new List<UserToken>();
        Email = email;
        IsLoggedIn = isLoggedIn;
        CreatedAt = DateTime.UtcNow;
    }
    
    new User Create(UserId userId, RoleId roleId, string googleId, string firstName, string course, string email, 
        bool isLoggedIn) =>
        new(userId, roleId, googleId, firstName, course, email, isLoggedIn);
}