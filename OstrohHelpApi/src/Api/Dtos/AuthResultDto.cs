namespace Api.Dtos;

public class AuthResultDto
{
    public string Id { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string RoleId { get; set; } = null!;
    public string JwtToken { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
}