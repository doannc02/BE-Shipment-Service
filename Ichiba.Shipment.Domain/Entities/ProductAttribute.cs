namespace Ichiba.Shipment.Domain.Entities;

public class ProductAttribute
{
    public required Guid Id { get; set; }
    public required Guid ProductId { get; set; }
    public required string Name { get; set; }
    public List<ProductAttributeValue> Values { get; set; } = new();
}
