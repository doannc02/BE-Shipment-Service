using Ichiba.Shipment.Application.Common.BaseResponse;
using Ichiba.Shipment.Domain.Consts;
using Ichiba.Shipment.Domain.Entities;
using Ichiba.Shipment.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Ichiba.Shipment.Application.PackageProducts.Commands;

public class CreatePackageProductCommandResponse
{
    public required Guid Id { get; set; }
}

public class CreatePackageProductCommand : IRequest<BaseEntity<CreatePackageProductCommandResponse>>
{
    public Guid? PackageId { get; set; }
    public string ProductName { get; set; }
    public Guid ProductId { get; set; }
    public string Origin { get; set; }
    public double OriginPrice { get; set; }
    public int Quantity { get; set; }
    public double Total { get; set; }
    public decimal Price { get; set; }
    public UnitProductType Unit { get; set; }
    public string ProductLink { get; set; }
    public decimal? Tax { get; set; }
}

public class CreatePkgProdCommandHandler : IRequestHandler<CreatePackageProductCommand, BaseEntity<CreatePackageProductCommandResponse>>
{
    private readonly ShipmentDbContext _dbContext;
    private readonly ILogger<CreatePkgProdCommandHandler> _logger;

    public CreatePkgProdCommandHandler(ShipmentDbContext dbContext, ILogger<CreatePkgProdCommandHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<BaseEntity<CreatePackageProductCommandResponse>> Handle(CreatePackageProductCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request.PackageId.HasValue)
            {
                var packageExists = await _dbContext.Packages
                    .AnyAsync(p => p.Id == request.PackageId.Value, cancellationToken);

                if (!packageExists)
                {
                    return new BaseEntity<CreatePackageProductCommandResponse>
                    {
                        Status = false,
                        Message = "Package không tồn tại."
                    };
                }
            }
            //else
            //{
            //    return new BaseEntity<CreatePackageProductCommandResponse>
            //    {
            //        Status = false,
            //        Message = "PackageId là bắt buộc."
            //    };
            //}

            var packageProduct = new PackageProduct
            {
                Id = Guid.NewGuid(),
                PackageId = request.PackageId,
                ProductName = request.ProductName,
                ProductId = request.ProductId,
                Origin = request.Origin,
                OriginPrice = request.OriginPrice,
                Quantity = request.Quantity,
                Total = request.OriginPrice * request.Quantity,
                Unit = request.Unit,
                ProductLink = request.ProductLink,
                Tax = request.Tax
            };

            await _dbContext.PackageProducts.AddAsync(packageProduct, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return new BaseEntity<CreatePackageProductCommandResponse>
            {
                Status = true,
                Message = "Tạo thành công.",
                Data = new CreatePackageProductCommandResponse { Id = packageProduct.Id }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi tạo PackageProduct.");
            return new BaseEntity<CreatePackageProductCommandResponse>
            {
                Status  = false,
                Message = "Đã xảy ra lỗi, vui lòng thử lại."
            };
        }
    }
}
