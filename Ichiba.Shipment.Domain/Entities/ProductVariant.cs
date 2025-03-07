namespace Ichiba.Shipment.Domain.Entities;

public class ProductVariant
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProductId { get; set; }
    public string SKU { get; set; }
    public decimal Price { get; set; }
    public decimal Weight { get; set; }
    public decimal Length { get; set; }
    public decimal Width { get; set; }
    public decimal Height { get; set; }
    public DateTime CreateAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdateAt { get; set; }
    public long? StockQty { get; set; }
    public virtual Product Product { get; set; }
    public List<ProductVariantImage> Images { get; set; } = new();
    public virtual ICollection<ProductVariantAtttributeValue> Attributes { get; set; }
}
