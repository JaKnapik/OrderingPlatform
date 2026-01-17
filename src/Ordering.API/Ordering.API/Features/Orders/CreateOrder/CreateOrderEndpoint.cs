using Microsoft.AspNetCore.Mvc;
namespace Ordering.API.Features.Orders.CreateOrder;


public static class CreateOrderEndpoint
{
	public static void MapCreateOrder(this IEndpointRouteBuilder app)
	{
		app.MapPost("/api/orders", async ([FromBody] CreateOrderRequest request, [FromServices] CreateOrderHandler handler) => 
		{
			var result = await handler.HandleAsync(request);
			return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
		}).RequireAuthorization().WithName("CreateOrder").AddOpenApiOperationTransformer((op, context, ct) =>
		{
			op.Summary = "Creating order";
			op.Description = "Returns orderId and total amount";
			return Task.CompletedTask;
		});
	}
}
