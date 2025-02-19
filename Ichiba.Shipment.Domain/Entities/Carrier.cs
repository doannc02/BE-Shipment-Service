namespace Ichiba.Shipment.Domain.Entities;

public class Carrier
{
    public required Guid Id { get; set; }
    public required string Code { get; set; }
    public bool? lastmile_tracking { get; set; }
    public string? logo { get; set; }
    public DateTime CreatedDate { get; set; }
    public string CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
    public ShippingMethod ShippingMethod { get; set; }
    public CarrierType Type { get; set; }
}

public enum CarrierType
{
    Express,
    Economy
}
public enum ShippingMethod
{
    Air,
    Occean,
    Inland
}
