using Ichiba.Shipment.Application.Common.BaseResponse;
using Ichiba.Shipment.Domain.Consts;
using Ichiba.Shipment.Domain.Entities;
using Ichiba.Shipment.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Ichiba.Shipment.Application.Packages;

public record UpdatePackageCommandResponse
{
    public Guid Id { get; set; }
}
public class UpdatePackageCommand : IRequest<BaseEntity<UpdatePackageCommandResponse>>
{
    public required Guid Id { get; set; }
    public PackageStatus PackageStatus { get; set; }
    public Guid? ShipmentAddressId { get; set; }
    public string? Note { get; set; }
    public decimal Length { get; set; }
    public decimal Width { get; set; }
    public decimal Height { get; set; }
    public decimal Weight { get; set; }
    public Guid? UpdateBy { get; set; }
}

public class UpdatePackageCommandHandler : IRequestHandler<UpdatePackageCommand, BaseEntity<UpdatePackageCommandResponse>>
{
    private readonly ShipmentDbContext _dbContext;
    private readonly ILogger<UpdatePackageCommandHandler> _logger;

    public UpdatePackageCommandHandler(ShipmentDbContext dbContext, ILogger<UpdatePackageCommandHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<BaseEntity<UpdatePackageCommandResponse>> Handle(UpdatePackageCommand request, CancellationToken cancellationToken)
    {
        var package = await CheckExistPackage(request.Id);
        if (package == null)
        {
            return new BaseEntity<UpdatePackageCommandResponse>
            {
                Status = false,
                Message = "Package not found."
            };
        }

        if (!await ValidateAllowUpdate(package.Status))
        {
            return new BaseEntity<UpdatePackageCommandResponse>
            {
                Status = false,
                Message = "Cannot update a package that has been shipped or delivered."
            };
        }

        if (request.ShipmentAddressId != Guid.Empty && request.ShipmentAddressId != null)
        {
            var shipmentAddress = await _dbContext.ShipmentAddresses.FindAsync(request.ShipmentAddressId);
            if (shipmentAddress == null)
            {
                return new BaseEntity<UpdatePackageCommandResponse>
                {
                    Status = false,
                    Message = "Shipment address not found."
                };
            }
        }
        package.Status = request.PackageStatus;
        package.Note = request.Note;
        package.Length = request.Length;
        package.Width = request.Width;
        package.Height = request.Height;
        package.Weight = request.Weight;
        package.UpdateAt = DateTime.UtcNow;
        package.UpdateBy = request.UpdateBy;

        _dbContext.Packages.Update(package);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await UpdateShipmentTotalWeightAndHeight(package.Id);

        return new BaseEntity<UpdatePackageCommandResponse>
        {
            Status = true,
            Message = "Package updated successfully.",
            Data = new UpdatePackageCommandResponse { Id = package.Id }
        };
    }

    private async Task<Package?> CheckExistPackage(Guid id)
    {
        return await _dbContext.Packages.Include(i => i.PackageAdresses).SingleOrDefaultAsync(i => i.Id == id);
    }

    private async Task<bool> ValidateAllowUpdate(PackageStatus status)
    {
        return status != PackageStatus.Delevred ;
    }

    private async Task UpdateShipmentTotalWeightAndHeight(Guid packageId)
    {
        var package = await _dbContext.Packages.FirstOrDefaultAsync(p => p.Id == packageId);
        if (package == null)
        {
            throw new Exception("Package not found.");
        }

        var shipmentId = await _dbContext.ShipmentPackages
            .Where(i => i.PackageId == packageId)
            .Select(i => i.ShipmentId)
            .FirstOrDefaultAsync();

        if (shipmentId == Guid.Empty)
        {
            throw new Exception("Shipment not found for this package.");
        }

        var shipmentPackages = await _dbContext.ShipmentPackages
            .Where(i => i.ShipmentId == shipmentId)
            .Select(i => i.PackageId) 
            .ToListAsync();

        var totals = await _dbContext.Packages
            .Where(p => shipmentPackages.Contains(p.Id)) 
            .GroupBy(p => 1) 
            .Select(g => new
            {
                TotalHeight = g.Sum(p => p.Height),
                TotalWeight = g.Sum(p => p.Weight)
            })
            .FirstOrDefaultAsync();

        if (totals == null)
        {
            throw new Exception("No packages found for the shipment.");
        }

        // Cập nhật thông tin của Shipment
        var shipment = await _dbContext.Shipments.FirstOrDefaultAsync(s => s.Id == shipmentId);
        if (shipment != null)
        {
            shipment.Weight = totals.TotalWeight;
            shipment.Height = totals.TotalHeight;

            // Lưu thay đổi vào cơ sở dữ liệu
            await _dbContext.SaveChangesAsync();
        }
    }


}
