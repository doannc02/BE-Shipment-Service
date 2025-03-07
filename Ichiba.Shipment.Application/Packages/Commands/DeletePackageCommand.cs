using Ichiba.Shipment.Application.Common.BaseResponse;
using Ichiba.Shipment.Domain.Consts;
using Ichiba.Shipment.Infrastructure.Data;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Ichiba.Shipment.Application.Packages.Commands;

public class DeletePackageResponse
{
    public Guid Id { get; set; }
}
public class DeletePackageCommand : IRequest<BaseEntity<DeletePackageCommand>>
{
    public required Guid Id { get; set; }
}

public class DeletePackageCommandHandler : IRequestHandler<DeletePackageCommand, BaseEntity<DeletePackageCommand>>
{
    private readonly ShipmentDbContext _dbContext;
    private readonly ILogger<DeletePackageCommandHandler> _logger;

    public DeletePackageCommandHandler(ShipmentDbContext dbContext, ILogger<DeletePackageCommandHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<BaseEntity<DeletePackageCommand>> Handle(DeletePackageCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var package = await _dbContext.Packages.FindAsync(request.Id);
            if (package == null)
            {
                return new BaseEntity<DeletePackageCommand>
                {
                    Status = false,
                    Message = "Package not found."
                };
            }

            if (package.Status == PackageStatus.Delevred )
            {
                return new BaseEntity<DeletePackageCommand>
                {
                    Status = false,
                    Message = "Cannot delete a package that has been shipped or delivered."
                };
            }

            _dbContext.Packages.Remove(package);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return new BaseEntity<DeletePackageCommand>
            {
                Status = true,
                Message = "Package deleted successfully."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting package");
            return new BaseEntity<DeletePackageCommand>
            {
                Status = false,
                Message = "An error occurred while deleting the package."
            };
        }
    }
}
