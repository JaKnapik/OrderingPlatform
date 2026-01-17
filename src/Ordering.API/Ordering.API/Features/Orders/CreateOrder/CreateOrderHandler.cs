using MassTransit;
using Microsoft.EntityFrameworkCore;
using Ordering.API.Common;
using Ordering.API.Domain;
using Ordering.API.Infrastructure.Data;
using System.Diagnostics;
using System.Security.Claims;
using System.Text.Json;

namespace Ordering.API.Features.Orders.CreateOrder;

public class CreateOrderHandler(OrderingContext context, IHttpContextAccessor httpContextAccessor, IPublishEndpoint publishEndpoint, ILogger<CreateOrderHandler> logger)
{
	public async Task<ApiResponse<CreateOrderResponse>> HandleAsync(CreateOrderRequest request)
	{
		var userId = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
		if (string.IsNullOrEmpty(userId)) return Result.Failure<CreateOrderResponse>("Unauthorized");

		using var transaction = await context.Database.BeginTransactionAsync();

		try
		{
			var storeExists = await context.Stores.AnyAsync(s => s.StoreCode == request.StoreCode);
			if (!storeExists) return Result.Failure<CreateOrderResponse>("Store not found.");

			var order = new Order
			{
				Id = Guid.NewGuid(),
				UserId = userId,
				StoreCode = request.StoreCode,
				TotalAmount = request.Items.Sum(i => i.Quantity * i.UnitPrice),
				Items = request.Items.Select(i => new OrderItem
				{
					Id = Guid.NewGuid(),
					ProductId = i.ProductId,
					Quantity = i.Quantity,
					UnitPrice = i.UnitPrice,
				}).ToList()
			};
			context.Entry(order).Property("LastModifiedBy").CurrentValue = userId;

			context.Orders.Add(order);
			var orderCreatedEvent = new OrderCreatedEvent(order.Id,
				order.TotalAmount,
				order.UserId,
				order.StoreCode);
			await publishEndpoint.Publish(orderCreatedEvent);
			

			//var outboxMessage = new OutboxMessage
			//{
			//	Id = Guid.NewGuid(),
			//	Type = "OrderCreated",
			//	Content = JsonSerializer.Serialize(orderCreatedEvent),
			//	OccurredOnUtc = DateTime.UtcNow,
			//	TraceId = Activity.Current?.TraceId.ToString() ?? httpContextAccessor.HttpContext?.TraceIdentifier,
			//	CorrelationId = Guid.NewGuid(),
			//};
			//context.OutboxMessages.Add(outboxMessage);

			await context.SaveChangesAsync();
			await transaction.CommitAsync();

			return Result.Success(new CreateOrderResponse(order.Id, order.TotalAmount));
		}
		catch (Exception ex)
		{
			await transaction.RollbackAsync();
			logger.LogError(ex, "Error while creating order. Transaction rolled back");
			throw;
		}
	}
}
public record OrderCreatedEvent(Guid OrderId, decimal TotalAmount, string UserId, string StoreCode);
