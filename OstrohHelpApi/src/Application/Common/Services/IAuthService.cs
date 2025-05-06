using Domain.Users;

namespace Application.Services.Interface;

public interface IAuthService
{
    string GenerateJwtToken(User user);
    string GenerateRefreshToken();
}