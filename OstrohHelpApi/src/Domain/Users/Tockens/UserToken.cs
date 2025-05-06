namespace Domain.Users.Tockens;

public class UserToken
{
    public UserTockenId Id { get; set; }

    public User User { get; set; }
    public UserId UserId { get; set; }
    public string RefreshToken { get; set; }
    public DateTime ExpiresAt { get; set; }

    UserToken(UserTockenId id, UserId userId, string refreshToken, DateTime expiresAt)
    {
        Id = id;
        UserId = userId;
        RefreshToken = refreshToken;
        ExpiresAt = expiresAt;
    }
    
    new UserToken Create(UserTockenId id, UserId userId, string refreshToken, DateTime expiresAt) =>
        new(id, userId, refreshToken, expiresAt);
}