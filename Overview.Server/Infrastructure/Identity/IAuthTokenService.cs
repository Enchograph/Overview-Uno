namespace Overview.Server.Infrastructure.Identity;

public interface IAuthTokenService
{
    AuthTokenPair CreateTokenPair(Guid userId, string email);

    string HashRefreshToken(string refreshToken);
}
