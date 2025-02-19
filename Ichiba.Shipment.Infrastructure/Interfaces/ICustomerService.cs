using Ichiba.Shipment.Infrastructure.Services.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ichiba.Shipment.Infrastructure.Interfaces
{
    public interface ICustomerService
    {
        Task<CustomerEntityView> GetDetailCustomer(Guid idCustomer);
    }
}
