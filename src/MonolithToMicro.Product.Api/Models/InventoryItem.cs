namespace MonolithToMicro.Product.Api.Models;
public sealed class InventoryItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int AvailableQuantity { get; set; }
    public decimal UnitPrice { get; set; }
}
