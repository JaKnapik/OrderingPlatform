namespace Ordering.API.Features.Auth.Register;

public record RegisterRequest(string Email, string Password, string FirstName, string LastName);

public record RegisterResponse(Guid UserId, string Email);