using Ichiba.Shipment.Application.Common.BaseResponse;
using Ichiba.Shipment.Domain.Interfaces;
using Ichiba.Shipment.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Ichiba.Shipment.Application.Shipments.Commands;

public class UpdateShipmentPackageResponse
{
    public Guid Id { get; set; }
}

public class UpdateShipmentPackageCommand : IRequest<BaseEntity<UpdateShipmentPackageResponse>>
{
    public required Guid Id { get; set; }
    //public string? Note { get; set; }
    //public required Guid UpdatedBy { get; set; }
    //public decimal? TotalAmount { get; set; }
    //public decimal? Weight { get; set; }
    //public decimal? Height { get; set; }
}

public class UpdateShipmentPackageCommandHandler : IRequestHandler<UpdateShipmentPackageCommand, BaseEntity<UpdateShipmentPackageResponse>>
{
    private readonly ILogger<UpdateShipmentPackageCommandHandler> _logger;
    private readonly ShipmentDbContext _dbContext;
    public UpdateShipmentPackageCommandHandler(ILogger<UpdateShipmentPackageCommandHandler> logger, ShipmentDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task<BaseEntity<UpdateShipmentPackageResponse>> Handle(UpdateShipmentPackageCommand request, CancellationToken cancellationToken)
    {
        if (request.Id == Guid.Empty)
        {
            return new BaseEntity<UpdateShipmentPackageResponse>
            {
                Status = false,
                Message = "Shipment ID không hợp lệ."
            };
        }

        var entity = await _dbContext.ShipmentPackages.SingleOrDefaultAsync(i => i.Id == request.Id);
        if (entity == null)
        {
            return new BaseEntity<UpdateShipmentPackageResponse>
            {
                Status = false,
                Message = "Không tìm thấy shipment  package."
            };
        }

        _dbContext.ShipmentPackages.Update(entity);

        _logger.LogInformation($"Shipment package {entity.Id} updated successfully.");

        return new BaseEntity<UpdateShipmentPackageResponse>
        {
            Data = new UpdateShipmentPackageResponse { Id = entity.Id },
            Message = "Cập nhật thành công.",
            Status = true
        };
    }
}
