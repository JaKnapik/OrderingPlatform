using Ordering.API.Infrastructure.Auth;

namespace Ordering.API.Domain;

public class Order
{
	public Guid Id { get; set; }
	public DateTime OrderDate { get; set; } = DateTime.UtcNow;
	public decimal TotalAmount { get; set; }

	public string UserId { get; set; } = string.Empty;
	public ApplicationUser User { get; set; } = null!;

	public string StoreCode { get; set; } = string.Empty;
	public Store Store { get; set; } = null!;

	public ICollection<OrderItem> Items { get; set; } = [];
}

public class OrderItem
{
	public Guid Id { get; set; }
	public Guid ProductId { get; set; }
	public int Quantity { get; set; }
	public decimal UnitPrice { get; set; }
	public Guid OrderId { get; set; }
	public Order Order { get; set; } = null!;
}
