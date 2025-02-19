namespace Ichiba.Shipment.Domain.Entities;

public class ShipmentPackage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ShipmentId { get; set; }
    public Guid PackageId { get; set; }
    public virtual Package Package { get; set; }
    public virtual ShipmentEntity Shipment { get; set; }
    public DateTime CreateAt { get; set; } = DateTime.UtcNow;
    public Guid? CreateBy { get; set; }
    public DateTime? UpdateAt { get; set; }
    public Guid? UpdateBy { get; set; }
    public DateTime? DeleteAt { get; set; }
    public Guid? DeleteBy { get; set; }
}
