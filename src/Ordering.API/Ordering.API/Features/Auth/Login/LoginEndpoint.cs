namespace Ordering.API.Features.Auth.Login;
using Microsoft.AspNetCore.Mvc;
using Ordering.API.Common;
using Ordering.API.Infrastructure.Auth;
using Microsoft.AspNetCore.Identity;
using FluentValidation;
public static class LoginEndpoint
{
    public static void MapLogin(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/auth/login", async ([FromBody] LoginRequest request, [FromServices] UserManager<ApplicationUser> userManager, [FromServices] ITokenService tokenService, [FromServices] IValidator<LoginRequest> validator) =>
        {
            var validationResult = await validator.ValidateAsync(request);

            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToArray();
                var failure = Result.Failure<LoginResponse>(errors);
                return Results.BadRequest(failure);
            }

            var user = await userManager.FindByEmailAsync(request.Email);
            if (user is null || !await userManager.CheckPasswordAsync(user, request.Password))
            {
                return Results.Unauthorized();
            }

            var roles = await userManager.GetRolesAsync(user);

            var accessToken = tokenService.GenerateAccessToken(user, roles);

            var refreshToken = tokenService.GenerateRefreshToken();

            user.RefreshTokens.Add(refreshToken);
            await userManager.UpdateAsync(user);

            tokenService.SetRefreshTokenCookie(refreshToken.Token);

            var response = new LoginResponse(accessToken, user.Email!, user.FirstName, user.LastName);

            return Results.Ok(Result.Success(response, "Login successfull"));
        }).WithName("Login").AddOpenApiOperationTransformer((op, context, ct) =>
        {
            op.Summary = "Login";
            op.Description = "Return short-lived jwt access token and long-lived refresh token http cookie only ";
            return Task.CompletedTask;
        });
    }
}
