namespace Ichiba.Shipment.Domain.Entities;

public class ProductAttributeValue
{
    public required Guid Id { get; set; }
    public Guid ProductAttributeId {get; set;}
    public ProductAttribute ProductAttribute { get; set; }
    public string Value { get; set; }
}
