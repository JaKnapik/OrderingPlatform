
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Ordering.API.Infrastructure.Auth;

public class TokenService(IConfiguration configuration, IHttpContextAccessor httpContextAccessor) : ITokenService
{
    public string GenerateAccessToken(ApplicationUser user, IList<string> roles)
    {
		var claims = new List<Claim>
		{
			new(ClaimTypes.NameIdentifier, user.Id),
			new(ClaimTypes.Email, user.Email),
			new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
		};
		foreach (var role in roles)
		{
			claims.Add(new Claim(ClaimTypes.Role, role));
		}
		var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]));
		var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

		var token = new JwtSecurityToken(
			issuer: configuration["Jwt:Issuer"],
			audience: configuration["Jwt:Audience"],
			claims: claims,
			expires: DateTime.UtcNow.AddMinutes(15),
			signingCredentials: creds
			);
		return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public RefreshToken GenerateRefreshToken()
    {
        return new RefreshToken
		{
			Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
			ExpiresAt = DateTime.UtcNow.AddDays(1),
		};
    }

    public void SetRefreshTokenCookie(string token)
	{
		var cookieOptions = new CookieOptions
		{
			HttpOnly = true,
			Secure = true,
			SameSite = SameSiteMode.Strict,
			Expires = DateTime.UtcNow.AddDays(1),
			Path = "/"
		};
		httpContextAccessor.HttpContext?.Response.Cookies.Append("refreshToken", token, cookieOptions);
	}
	
}
