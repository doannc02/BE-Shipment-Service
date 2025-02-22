using Ichiba.Shipment.Application.Common.BaseResponse;
using Ichiba.Shipment.Infrastructure.Data;
using Ichiba.Shipment.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace Ichiba.Shipment.Application.Warehouses.Commands
{
    public class CreateWarehouseCommandResponse
    {
        public Guid Id { get; set; }
    }

    public class CreateWarehouseCommand : IRequest<BaseEntity<CreateWarehouseCommandResponse>>
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Logo { get; set; }
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
        public required Guid CreateBy { get; set; }
        public string? Country { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }

    public class CreateWarehouseCommandHandler : IRequestHandler<CreateWarehouseCommand, BaseEntity<CreateWarehouseCommandResponse>>
    {
        private readonly ShipmentDbContext _dbContext;
        private readonly ILogger<CreateWarehouseCommandHandler> _logger;

        public CreateWarehouseCommandHandler(ShipmentDbContext dbContext, ILogger<CreateWarehouseCommandHandler> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<BaseEntity<CreateWarehouseCommandResponse>> Handle(CreateWarehouseCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var warehouse = await _dbContext.Warehouses
             .Where(w => w.Code == request.Code && w.Name == request.Name)
            .FirstOrDefaultAsync(cancellationToken);



                if (warehouse != null)
                {
                    return new BaseEntity<CreateWarehouseCommandResponse>
                    {
                        Status = false,
                        Message = "A warehouse with the same Name, Code, or Phone already exists.",
                        Data = null
                    };
                }

                var newWarehouse = new Warehouse
                {
                    Id = request.Id,
                    Name = request.Name,
                    Logo = request.Logo,
                    PrefixPhone = request.PrefixPhone,
                    PhoneNumber = request.PhoneNumber,
                    Code = request.Code,
                    Phone = request.Phone,
                    City = request.City,
                    District = request.District,
                    Ward = request.Ward,
                    PostCode = request.PostCode,
                    Address = request.Address,
                    CreateAt = DateTime.UtcNow,
                    CreateBy = request.CreateBy,
                    Country = request.Country,
                    Latitude = request.Latitude,
                    Longitude = request.Longitude
                };

                _dbContext.Warehouses.Add(newWarehouse);
                await _dbContext.SaveChangesAsync(cancellationToken);

                return new BaseEntity<CreateWarehouseCommandResponse>
                {
                    Status = true,
                    Message = "Warehouse created successfully.",
                    Data = new CreateWarehouseCommandResponse
                    {
                        Id = newWarehouse.Id
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating warehouse: {ex.Message}");
                return new BaseEntity<CreateWarehouseCommandResponse>
                {
                    Status = false,
                    Message = "Error occurred while creating the warehouse.",
                    Data = null
                };
            }
        }
    }
}
