namespace Ordering.API.Domain;

public class Store
{
	public Guid Id { get; set; }
	public string Name { get; set; } = string.Empty;
	public string StoreCode { get; set; } = string.Empty;
	public ICollection<Order> Orders { get; set; } = [];
}
