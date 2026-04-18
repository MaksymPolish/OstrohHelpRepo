namespace Domain.Users.Tockens;

public class UserToken
{
    public Guid Id { get; set; }

    public User User { get; set; }
    public Guid UserId { get; set; }
    public string RefreshToken { get; set; }
    public DateTime ExpiresAt { get; set; }

    UserToken(Guid id, Guid userId, string refreshToken, DateTime expiresAt)
    {
        Id = id;
        UserId = userId;
        RefreshToken = refreshToken;
        ExpiresAt = expiresAt;
    }
    
    new UserToken Create(Guid id, Guid userId, string refreshToken, DateTime expiresAt) =>
        new(id, userId, refreshToken, expiresAt);
}