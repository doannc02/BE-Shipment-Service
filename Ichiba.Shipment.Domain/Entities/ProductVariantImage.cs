namespace Ichiba.Shipment.Domain.Entities;

public class ProductVariantImage
{
    public Guid Id { get; set; }
    public Guid PrroductVariantId { get; set; }
    public ProductVariant ProductVariant { get; set; }
    public string ImageUrl { get; set; }
}
