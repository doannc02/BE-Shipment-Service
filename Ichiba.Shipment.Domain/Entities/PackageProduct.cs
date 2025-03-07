using Ichiba.Shipment.Domain.Consts;

namespace Ichiba.Shipment.Domain.Entities;
// làm đơn vận chuyển hộ
public class PackageProduct
{
    public required Guid Id { get; set; }
    public Guid? PackageId { get; set; }
    public virtual Package? Package { get; set; }
    public virtual Product Product { get; set; }
    public string ProductName { get; set; }
    public Guid? ProductId { get; set; }
    public string? Origin { get; set; }
    public double? OriginPrice { get; set; }
    public int Quantity { get; set; }
    public double Total { get; set; }
    public UnitProductType? Unit { get; set; } // item, set
    public string? ProductLink { get; set; }
    public decimal? Tax { get; set; }
}
