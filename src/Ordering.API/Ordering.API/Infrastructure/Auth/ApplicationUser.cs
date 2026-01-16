using Microsoft.AspNetCore.Identity;

namespace Ordering.API.Infrastructure.Auth;
public class ApplicationUser : IdentityUser
{
	public string FirstName { get; set; } = string.Empty;
	public string LastName { get; set; } = string.Empty;
	public List<RefreshToken> RefreshTokens { get; set; } = [];
}

public class RefreshToken
{
	public string Token { get; set; } = string.Empty;
	public DateTime ExpiresAt {  get; set; }
	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
	public bool IsRevoked { get; set; }
	public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
	public bool IsActive => !IsRevoked && !IsExpired;
}
