using Ichiba.Shipment.Application.Common.BaseResponse;
using Ichiba.Shipment.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Ichiba.Shipment.Application.Shipments.Commands;

public record DeleteShipmentCommandResponse
{
    public Guid Id { get; set; }
}

public record DeleteShipmentCommand : IRequest<BaseEntity<DeleteShipmentCommandResponse>>
{
    public Guid Id { get; set; }
}

public class DeleteShipmentCommandHandler : IRequestHandler<DeleteShipmentCommand, BaseEntity<DeleteShipmentCommandResponse>>
{
    private readonly ShipmentDbContext _dbContext;
    private readonly ILogger<DeleteShipmentCommandHandler> _logger;
    public DeleteShipmentCommandHandler(ShipmentDbContext dbContext, ILogger<DeleteShipmentCommandHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }
    public async Task<BaseEntity<DeleteShipmentCommandResponse>> Handle(DeleteShipmentCommand request, CancellationToken cancellationToken)
    {

        var shipment = await _dbContext.Shipments
            .Include(s => s.ShipmentPackages)
            .FirstOrDefaultAsync(s => s.Id == request.Id);

        if (shipment == null)
        {
            return new BaseEntity<DeleteShipmentCommandResponse>
            {
                Message = "Not found shiment",
                Status = false
            };
        }

        _dbContext.ShipmentPackages.RemoveRange(shipment.ShipmentPackages);
        _dbContext.Shipments.Remove(shipment);
        await _dbContext.SaveChangesAsync();
        return new BaseEntity<DeleteShipmentCommandResponse>
        {
            Data = new DeleteShipmentCommandResponse
            {
                Id = request.Id
            },
            Message = "Delete shipment success!",
            Status = true
        };


    }
}



