using Ichiba.Shipment.Domain.Entities;
using Ichiba.Shipment.Domain.Interfaces;
using Ichiba.Shipment.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;


namespace Ichiba.Shipment.Infrastructure.Repositories
{
    public class ShipmentRepository : IShipmentRepository
    {
        private readonly ShipmentDbContext _dbContext;

        public ShipmentRepository(ShipmentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddAsync(ShipmentEntity shipment)
        {
            await _dbContext.Shipments.AddAsync(shipment);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateAsync(ShipmentEntity shipment)
        {
            _dbContext.Shipments.Update(shipment);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(ShipmentEntity shipment)
        {
            _dbContext.Shipments.Remove(shipment);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<ShipmentEntity?> GetAsync(Guid id)
        {
            return await _dbContext.Shipments.AsNoTracking().Include(i => i.Addresses).SingleOrDefaultAsync(a => a.Id == id);
        }

        public async Task<bool> ShipmentNumberExistsAsync(string shipmentNumber)
        {
            return await _dbContext.Shipments.AnyAsync(s => s.ShipmentNumber == shipmentNumber);
        }
    }
}
