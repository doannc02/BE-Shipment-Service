﻿namespace Ichiba.Shipment.Domain.Entities;

public class Package
{
    public Guid CustomerId { get; set; }
    public virtual ShipmentAddress ShipmentAddress { get; set; }
    public required Guid ShipmentAddressId { get; set; }
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid WarehouseId { get; set; }
    public string PackageNumber { get; set; } = string.Empty;
    public string? Note { get; set; }
    public PackageStatus Status { get; set; }
    public decimal Length { get; set; }
    public decimal Width { get; set; }
    public decimal Height { get; set; }
    public decimal Weight { get; set; }
    public DateTime CreateAt { get; set; } = DateTime.UtcNow;
    public Guid? CreateBy { get; set; }
    public DateTime? UpdateAt { get; set; }
    public Guid? UpdateBy { get; set; }
    public DateTime? DeleteAt { get; set; }
    public Guid? DeleteBy { get; set; }
}

public enum PackageStatus
{
    Pending,
    Processing,
    Shipped,
    Delivered,
    Returned,
    Cancelled
}
