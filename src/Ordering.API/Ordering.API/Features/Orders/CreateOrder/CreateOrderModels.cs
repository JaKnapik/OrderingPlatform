namespace Ordering.API.Features.Orders.CreateOrder;

public record OrderItemRequest(Guid ProductId, int Quantity, decimal UnitPrice);
public record CreateOrderRequest(string StoreCode, List<OrderItemRequest> Items);
public record CreateOrderResponse(Guid OrderId, decimal TotalAmount);


