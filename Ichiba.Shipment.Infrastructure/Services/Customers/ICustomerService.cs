using Ichiba.Shipment.Infrastructure.Services.Models;

namespace Ichiba.Shipment.Infrastructure.Services.Customers;
public interface ICustomerService
{
    Task<CustomerEntityView> GetDetailCustomer(Guid idCustomer);
}

public interface ICustomerBatchLookupService
{
    Task<Dictionary<Guid, CustomerEntityView>> GetListCustomerByIds(List<Guid> CustomerIds);
    Task<Dictionary<Guid, CustomerAddressView>> GetListAddressByCustomerIds(List<Guid> CustomerIds);
}
