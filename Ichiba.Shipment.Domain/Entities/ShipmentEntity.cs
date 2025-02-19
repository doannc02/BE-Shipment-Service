namespace Ichiba.Shipment.Domain.Entities;
public enum ShipmentStatus
{
    ShipmentCreated,
    InStorage,
    Processing,
    Processed,
    Delivering,
    Delivered
}
public class ShipmentEntity
{
    public Guid Id { get; set; }
    public Guid WarehouseId { get; set; }
    public Guid CustomerId { get; set; }
    public string ShipmentNumber { get; set; } = string.Empty;
    public string? Note { get; set; }
    public ShipmentStatus Status { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal Weight { get; set; }
    public decimal Height { get; set; }
    public DateTime CreateAt { get; set; } = DateTime.UtcNow;
    public Guid? CreateBy { get; set; }
    public DateTime? UpdateAt { get; set; }
    public Guid? UpdateBy { get; set; }
    public DateTime? DeleteAt { get; set; }
    public Guid? DeleteBy { get; set; }
    public virtual List<ShipmentAddress> Addresses { get; set; } = new();
    public virtual List<ShipmentPackage> ShipmentPackages { get; set; } = new();
}



