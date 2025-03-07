namespace Ichiba.Shipment.Domain.Entities;

public class Product
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty; 
    public string Code { get; set; }
    public string SKU { get; set; }
    public string? MetaTitle { get; set;  }
    public string Description { get; set; }
    public string Category { get; set; }
    public string Brand { get; set; }
    public string ImageUrl { get; set; }
    public decimal? Price { get; set; }
    public bool? IsHasVariant { get; set; }
    public DateTime CreateAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdateAt { get; set; }
    public virtual ICollection<ProductAttribute> ProductAttributes { get; set; }
    public virtual ICollection<ProductVariant> Variants { get; set; }
    public virtual ICollection<PackageProduct> PackageProducts { get; set; }
}
