using Ichiba.Shipment.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ichiba.Shipment.Domain.Interfaces
{
    public interface IShipmentRepository
    {
        Task<ShipmentEntity> GetAsync(Guid id);
        Task AddAsync(ShipmentEntity sampleEntity);
        Task UpdateAsync(ShipmentEntity sampleEntity);
        Task DeleteAsync(ShipmentEntity sampleEntity);
        Task<bool> ShipmentNumberExistsAsync(string shipmentNumber);

    }
}
