using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Ordering.API.Common;
using Ordering.API.Infrastructure.Auth;

namespace Ordering.API.Features.Auth.Register;

public static class RegisterEndpoint
{
    public static void MapRegister(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/auth/register", async ([FromBody] RegisterRequest request, [FromServices] UserManager<ApplicationUser> userManager, [FromServices] IValidator<RegisterRequest> validator) => 
        { 
            var validationResult = await validator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(x=>x.ErrorMessage).ToArray();
                var failure = Result.Failure<RegisterResponse>(errors);
                return Results.BadRequest(failure);
            }

            ApplicationUser? user = await userManager.FindByEmailAsync(request.Email);
            if(user is not null)
            {
                var failure = Result.Failure<RegisterResponse>("Email exists");
                return Results.BadRequest(failure);
            }

            user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
            };

            var result = await userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToArray();
                var failure = Result.Failure<RegisterResponse>(errors);
                return Results.BadRequest(failure);
            }

            await userManager.AddToRoleAsync(user, "User");
            var response = new RegisterResponse(Guid.Parse(user.Id), user.Email);
            return Results.Ok(Result.Success(response, "User registered successfully"));
        }
        ).WithName("Register").
        AddOpenApiOperationTransformer((op, context, ct) => 
        { 
            op.Summary = "Registration"; 
            op.Description = "Returns Guid and email address"; 
            return Task.CompletedTask; 
        });
    }
}
