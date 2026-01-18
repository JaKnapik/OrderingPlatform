namespace Inventory.API.Domain;

public class ProductStock
    
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }

    public int AvailableQuantity { get; set; }

    public string LastUpdatedBy { get; set; } = string.Empty;
}
