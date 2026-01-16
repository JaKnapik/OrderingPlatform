namespace Ordering.API.Infrastructure.Auth;

public interface ITokenService
{
	string GenerateAccessToken(ApplicationUser user, IList<string> roles);
	RefreshToken GenerateRefreshToken();
	void SetRefreshTokenCookie(string token);
}