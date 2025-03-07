namespace Ichiba.Shipment.Domain.Entities;

public class ProductVariantAtttributeValue
{
    public required Guid Id { get; set; }
    public required Guid ProductVariantId { get; set; }
    public required Guid ProductAttributeValueId { get; set; }
    public virtual ProductVariant ProductVariant { get; set; }
    public virtual ProductAttributeValue ProductAttributeValue { get;set;}
}
