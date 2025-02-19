using Ichiba.Shipment.Application.Common.BaseResponse;
using Ichiba.Shipment.Domain.Entities;
using Ichiba.Shipment.Infrastructure.Data;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Ichiba.Shipment.Application.ShipmentPackages.Commands;

public class CreateMultiPackageCommandResponse
{
    public List<Guid> ShipmentIds { get; set; } = new();
}

public class CreateMultiPackageCommand : IRequest<BaseEntity<CreateMultiPackageCommandResponse>>
{
    public required List<CreateShipmentPackageCommand> Shipments { get; set; }
}

public class CreateMultiPackageCommandHandler : IRequestHandler<CreateMultiPackageCommand, BaseEntity<CreateMultiPackageCommandResponse>>
{
    private readonly ShipmentDbContext _dbContext;
    private readonly ILogger<CreateMultiPackageCommandHandler> _logger;

    public CreateMultiPackageCommandHandler(ShipmentDbContext dbContext, ILogger<CreateMultiPackageCommandHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<BaseEntity<CreateMultiPackageCommandResponse>> Handle(CreateMultiPackageCommand request, CancellationToken cancellationToken)
    {
        var response = new CreateMultiPackageCommandResponse();

        foreach (var shipmentCommand in request.Shipments)
        {
            var shipmentPackage = new ShipmentPackage
            {
                Id = Guid.NewGuid(),
                CreateAt = DateTime.UtcNow,
                CreateBy = shipmentCommand.CreateBy,
                PackageId = shipmentCommand.PackageId,
                ShipmentId = shipmentCommand.ShipmentId,
            };

            await _dbContext.ShipmentPackages.AddAsync(shipmentPackage, cancellationToken);
            response.ShipmentIds.Add(shipmentPackage.Id);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new BaseEntity<CreateMultiPackageCommandResponse>
        {
            Data = response,
            Status = true,
            Message = "Multiple shipment packages created successfully."
        };
    }
}