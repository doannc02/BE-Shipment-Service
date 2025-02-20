using Ichiba.Shipment.Domain.Consts;
using Ichiba.Shipment.Domain.Entities;

public class PackageAddress
{
    public Guid Id { get; set; }
    public Guid PackageId { get; set; }
    public virtual Package? Package { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? PrefixPhone { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Code { get; set; }
    public string? Phone { get; set; }
    public string? City { get; set; }
    public string? District { get; set; }
    public string? Ward { get; set; }
    public string? PostCode { get; set; }
    public string Address { get; set; } = string.Empty;
    public ShipmentAddressType Type { get; set; }
    public DateTime CreateAt { get; set; } = DateTime.UtcNow;
    public Guid? CreateBy { get; set; }
    public DateTime? UpdateAt { get; set; }
    public Guid? UpdateBy { get; set; }
    public DateTime? DeleteAt { get; set; }
    public Guid? DeleteBy { get; set; }

    public string? Country { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? DeliveryInstructions { get; set; }
    public bool IsDefault { get; set; } = false;
    public bool SensitiveDataFlag { get; set; } = false;
    public DateTime? EstimatedDeliveryDate { get; set; }
    public DateTime? DeliveryDate { get; set; }
    public AddressStatus Status { get; set; } = AddressStatus.Active;
    public string SearchIndex { get; set; } = string.Empty;
}

public enum AddressStatus
{
    Active,
    Inactive,
    Invalid,
    Pending
}

