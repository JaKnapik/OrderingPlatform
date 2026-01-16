namespace Ordering.API.Features.Auth.Login;

public record LoginRequest(string Email, string Password);

public record LoginResponse(string AccessToken, string Email, string FirstName, string LastName);
