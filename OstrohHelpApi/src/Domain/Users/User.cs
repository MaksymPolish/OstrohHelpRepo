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
   
    public User() => CreatedAt = DateTime.UtcNow;

    User(UserId userId, RoleId roleId, string googleId, string firstName, string lastName, string course, string email,
        string passwordHash, bool isLoggedIn, string authToken, DateTime? tokenExpiration)
    {
        Id = userId;
        RoleId = roleId;
        GoogleId = googleId;
        FirstName = firstName;
        LastName = lastName;
        Course = course;
        Email = email;
        PasswordHash = passwordHash;
        IsLoggedIn = isLoggedIn;
        AuthToken = authToken;
        TokenExpiration = tokenExpiration;
        CreatedAt = DateTime.UtcNow;
    }
    
    new User Create(UserId userId, RoleId roleId, string googleId, string firstName, string lastName, string course, string email,
        string passwordHash, bool isLoggedIn, string authToken, DateTime? tokenExpiration) =>
        new(userId, roleId, googleId, firstName, lastName, course, email, passwordHash, isLoggedIn, authToken, tokenExpiration);
}