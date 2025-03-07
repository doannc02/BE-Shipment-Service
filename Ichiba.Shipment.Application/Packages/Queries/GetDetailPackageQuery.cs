using Ichiba.Shipment.Application.Common.BaseRequest;
using Ichiba.Shipment.Application.Common.BaseResponse;
using Ichiba.Shipment.Application.Common.Mappings;
using Ichiba.Shipment.Application.Products.Handlers;
using Ichiba.Shipment.Application.Products.Helper;
using Ichiba.Shipment.Application.Shipments.Queries;
using Ichiba.Shipment.Domain.Consts;
using Ichiba.Shipment.Domain.Entities;
using Ichiba.Shipment.Infrastructure.Data;
using Ichiba.Shipment.Infrastructure.Services.Customers;
using Ichiba.Shipment.Infrastructure.Services.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Ichiba.Shipment.Application.Packages.Queries;

public class GetDetailPackageQueryResponse
{
    public required Guid Id { get; set; }
    public string? Note { get; set; }
    public decimal Length { get; set; }
    public decimal Width { get; set; }
    public decimal Height { get; set; }
    public decimal Weight { get; set; }
    public List<ProductDetailResponse> Products { get; set; } = new();
    public List<PackageProductResponse> PackageProduct { get; set; }
    public PackageAddressCreateSMDTO AddressSender { get; set; }
    public PackageAddressCreateSMDTO AddressReceive { get; set; }
    public virtual CustomerDTO Customer { get; set; }
    public virtual CarrierSMView Carrier { get; set; }
    public Guid? UpdateBy { get; set; }
}

public class PackageProductResponse
{
    public required Guid Id { get; set; }
    public string ProductName { get; set; }
    public string? Origin { get; set; }
    public double? OriginPrice { get; set; }
    public int Quantity { get; set; }
    public double Total { get; set; }
    public UnitProductType? Unit { get; set; } 
    public string? ProductLink { get; set; }
    public decimal? Tax { get; set; }
}

public record CustomerDTO
{
    public Guid Id { get; set; }
    public string FullName { get; set; }
}

public class GetDetailPackageQuery : QueryDetail, IRequest<BaseEntity<GetDetailPackageQueryResponse>>
{
    public Guid? CustomerId { get; set; }
}

public class GetDetailPackageQueryHandler : IRequestHandler<GetDetailPackageQuery, BaseEntity<GetDetailPackageQueryResponse>>
{
    private readonly ShipmentDbContext _dbContext;
    private readonly ILogger<GetDetailPackageQueryHandler> _logger;
    private readonly ICustomerService _customerService;

    public GetDetailPackageQueryHandler(ShipmentDbContext dbContext, ILogger<GetDetailPackageQueryHandler> logger, ICustomerService customerService)
    {
        _dbContext = dbContext;
        _logger = logger;
        _customerService = customerService;
    }

    public async Task<BaseEntity<GetDetailPackageQueryResponse>> Handle(GetDetailPackageQuery request, CancellationToken cancellationToken)
    {
        var packageQuery = _dbContext.Packages
            .AsNoTracking()
            .Include(p => p.PackageAdresses)
            .Include(p => p.PackageProducts)
                .ThenInclude(pp => pp.Product)
                    .ThenInclude(p => p.Variants)
                        .ThenInclude(v => v.Attributes)
            .Include(p => p.PackageProducts)
                .ThenInclude(pp => pp.Product)
                    .ThenInclude(p => p.ProductAttributes)
                        .ThenInclude(pa => pa.Values)
            .Where(p => p.Id == request.Id);

        var customer = new CustomerEntityView();
        if (request.CustomerId != Guid.Empty && request.CustomerId != null)
        {
            packageQuery = packageQuery.Where(p => p.CustomerId == request.CustomerId);
            customer = await GetCustomerById((Guid)request.CustomerId);
        }

        var packageDetail = await packageQuery
            .Select(p => new
            {
                p.Id,
                p.PackageAdresses,
                p.Note,
                p.Length,
                p.Width,
                p.Height,
                p.Weight,
                p.UpdateBy,
                p.CustomerId,
                p.PackageProducts,
                Customer = new CustomerDTO
                {
                    FullName = customer.FullName,
                    Id = customer.Id
                },
                p.CarrierId,
                Products = p.PackageProducts != null && p.PackageProducts.Any()
            ? p.PackageProducts.Select(pp => pp.Product).Where(product => product != null).ToList()
            : new List<Product>()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (packageDetail == null)
        {
            return new BaseEntity<GetDetailPackageQueryResponse>
            {
                Status = false,
                Message = "Package not found."
            };
        }

        var carrier = await _dbContext.Carriers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == packageDetail.CarrierId, cancellationToken);


        var packageProductDetails = packageDetail.PackageProducts?.Any() == true ? packageDetail.PackageProducts.Select(pk => new PackageProductResponse { 
            Id = pk.Id,
            ProductName = pk.ProductName,
            Origin = pk.Origin,
            OriginPrice = pk.OriginPrice,
            ProductLink = pk.ProductLink,
            Quantity = pk.Quantity,
            Tax = pk.Tax,
            Total = pk.Total,
            Unit = pk.Unit
        
        }).ToList() : new List<PackageProductResponse>();

        // Ánh xạ thông tin sản phẩm vào ProductDetailResponse
        var productDetails = packageDetail.Products?.Any() == true  || packageDetail.Products == null 
    ? packageDetail.Products
        .Select(p => new ProductDetailResponse
        {
            Id = p?.Id ?? Guid.Empty,
            Name = p?.Name ?? "Unknown",
            Brand = p?.Brand ?? "Unknown",
            Category = p?.Category ?? "Unknown",
            Code = p?.Code ?? "Unknown",
            SKU = p?.SKU ?? "Unknown",
            ImageUrl = p?.ImageUrl ?? "",
            Description = p?.Description ?? "",
            MetaTitle = p?.MetaTitle ?? "",
            IsHasVariant = p?.IsHasVariant ?? false,
            Price = p?.Price ?? 0,
            CreateAt = p?.CreateAt ?? DateTime.MinValue,
            Attributes = p?.ProductAttributes?
                .Select(pa => new ProductAttributeDetail
                {
                    Id = pa?.Id ?? Guid.Empty,
                    Name = pa?.Name ?? "Unknown",
                    Values = pa?.Values?.Select(v => v?.Value ?? "Unknown").ToList() ?? new List<string>()
                }).ToList() ?? new List<ProductAttributeDetail>(),
            Variants = p?.Variants?
                .Select(v => new ProductVariantDetail
                {
                    Id = v?.Id ?? Guid.Empty,
                    SKU = v?.SKU ?? "Unknown",
                    Price = v?.Price ?? 0,
                    Weight = v?.Weight ?? 0,
                    Length = v?.Length ?? 0,
                    Width = v?.Width ?? 0,
                    Height = v?.Height ?? 0,
                    StockQty = v?.StockQty ?? 0,
                    AttributeValues = v?.Attributes?
                        .Where(a => a?.ProductAttributeValue != null && a?.ProductAttributeValue?.ProductAttribute != null)
                        .ToDictionary(
                            a => a?.ProductAttributeValue?.ProductAttribute?.Name ?? "Unknown",
                            a => a?.ProductAttributeValue?.Value ?? "Unknown"
                        ) ?? new Dictionary<string, string>(),
                    ImageUrls = v?.Images?.Select(i => i?.ImageUrl ?? "").ToList() ?? new List<string>()
                }).ToList() ?? new List<ProductVariantDetail>()
        }).ToList()
    : new List<ProductDetailResponse>();  



        return new BaseEntity<GetDetailPackageQueryResponse>
        {
            Status = true,
            Message = "Package details retrieved successfully.",
            Data = new GetDetailPackageQueryResponse
            {
                Id = packageDetail.Id,
                AddressReceive = PackageAddressMapping.MapPackageAddress(packageDetail.PackageAdresses.FirstOrDefault(i => i.Type == ShipmentAddressType.ReceiveAddress)),
                AddressSender = PackageAddressMapping.MapPackageAddress(packageDetail.PackageAdresses.FirstOrDefault(i => i.Type == ShipmentAddressType.SenderAddress)),
                Note = packageDetail.Note,
                Length = packageDetail.Length,
                Width = packageDetail.Width,
                Height = packageDetail.Height,
                Weight = packageDetail.Weight,
                UpdateBy = packageDetail.UpdateBy,
                Customer = packageDetail.Customer,
                Carrier = carrier != null ? new CarrierSMView
                {
                    Id = carrier.Id,
                    Code = carrier.Code,
                    lastmile_tracking = carrier.lastmile_tracking,
                    logo = carrier.logo,
                    ShippingMethod = carrier.ShippingMethod,
                    Type = carrier.Type
                } : null!,
                Products = productDetails,
                PackageProduct = packageProductDetails
            }
        };
    }

    public async Task<CustomerEntityView> GetCustomerById(Guid idCustomer)
    {
        return await _customerService.GetDetailCustomer(idCustomer);
    }
}
