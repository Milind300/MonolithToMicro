namespace MonolithToMicro.Order.Api.Integrations;

public sealed class InventoryItemResponse
{
    public Guid Id { get; set; }

    public string Sku { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public int AvailableQuantity { get; set; }

    public decimal UnitPrice { get; set; }
}