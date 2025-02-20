using Ichiba.Shipment.Application.Common.BaseResponse;
using Ichiba.Shipment.Application.Packages.Commands;
using Ichiba.Shipment.Domain.Entities;
using Ichiba.Shipment.Infrastructure.Data;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Ichiba.Shipment.Application.Carriers.Commands;

public class CreateCarrierCommandResponse
{
    public Guid Id { get; set; }
}

public class CreateCarrierCommand : IRequest<BaseEntity<CreateCarrierCommandResponse>>
{
    public required string Code { get; set; }
    public bool? lastmile_tracking { get; set; } = false;
    public string? logo { get; set; }
    public string CreatedBy { get; set; }
    public ShippingMethod ShippingMethod { get; set; }
    public CarrierType Type { get; set; }
}

public class CreateCarrierCommandHandler : IRequestHandler<CreateCarrierCommand, BaseEntity<CreateCarrierCommandResponse>>
{
    private readonly ShipmentDbContext _dbContext;
    private readonly ILogger<CreateCarrierCommandHandler> _logger;
    public CreateCarrierCommandHandler(ShipmentDbContext dbContext, ILogger<CreateCarrierCommandHandler> logger)        
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<BaseEntity<CreateCarrierCommandResponse>> Handle(CreateCarrierCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var carrier = new Carrier
            {
                Id = Guid.NewGuid(),
                Code = request.Code,
                logo = request.logo,
                Type = request.Type,
                ShippingMethod = request.ShippingMethod,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = request.CreatedBy
            };

            await _dbContext.AddAsync(carrier, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation($"Carrier {carrier} created successfully.");

            return new BaseEntity<CreateCarrierCommandResponse>
            {
                Data = new CreateCarrierCommandResponse
                {
                    Id = carrier.Id
                },
                Status = true,
                Message = "Thêm mới thành công"
            };
        }catch(Exception ex)
        {
            _logger.LogError(ex, "Error deleting carrier");
            return new BaseEntity<CreateCarrierCommandResponse>
            {
                Status = false,
                Message = "An error occurred while deleting the carrier."
            };
        }
    }
}