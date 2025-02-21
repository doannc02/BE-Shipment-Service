namespace Ichiba.Shipment.Infrastructure.Services.Models;

public class CustomerEntityView
{
    public Guid Id { get; set; }
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool? isDelete { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public virtual ICollection<CustomerAddressView> Addresses { get; set; }
}
