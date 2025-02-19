using Ichiba.Shipment.Application.Common.BaseResponse;
using Ichiba.Shipment.Domain.Entities;
using Ichiba.Shipment.Infrastructure.Data;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Ichiba.Shipment.Application.ShipmentPackages.Commands;

public class CreateShipmentPackageCommandResponse
{
    public Guid Id { get; set; }
}

public class CreateShipmentPackageCommand : IRequest<BaseEntity<CreateShipmentPackageCommandResponse>>
{
    public Guid Id { get; set; }
    public Guid ShipmentId { get; set; }

    public Guid PackageId { get; set; }
    public Guid? CreateBy { get; set; }
}

public class CreateShipmentPackageCommandHandler : IRequestHandler<CreateShipmentPackageCommand, BaseEntity<CreateShipmentPackageCommandResponse>>
{
    private readonly ShipmentDbContext _dbContext;
    private readonly ILogger<CreateShipmentPackageCommandHandler> _logger;

    public CreateShipmentPackageCommandHandler(ShipmentDbContext dbContext, ILogger<CreateShipmentPackageCommandHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<BaseEntity<CreateShipmentPackageCommandResponse>> Handle(CreateShipmentPackageCommand request, CancellationToken cancellationToken)
    {

        var shipmentPackage = new ShipmentPackage
        {
            Id = Guid.NewGuid(),
            CreateAt = DateTime.UtcNow,
            CreateBy = request.CreateBy,
            PackageId = request.PackageId,
            ShipmentId = request.ShipmentId,
        };

        await _dbContext.ShipmentPackages.AddAsync(shipmentPackage, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var response = new CreateShipmentPackageCommandResponse
        {
            Id = shipmentPackage.Id,
        };

        return new BaseEntity<CreateShipmentPackageCommandResponse>
        {
            Data = response,
            Status = true,
            Message = "Shipment package created successfully."
        };
    }
}