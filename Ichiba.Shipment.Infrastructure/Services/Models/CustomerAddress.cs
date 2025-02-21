namespace Ichiba.Shipment.Infrastructure.Services.Models;

public class CustomerAddressView
{
    public Guid Id { get; set; }
    public string City { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string Ward { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
  //  public virtual CustomerEntityView? Customer { get; set; }
    public Guid CreateBy { get; set;  }
    public bool IsDefaultAddress { get; set; } = false;
    public string? Country { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}
