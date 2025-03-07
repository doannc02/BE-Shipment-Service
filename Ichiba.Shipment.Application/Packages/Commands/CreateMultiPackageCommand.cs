using Ichiba.Shipment.Application.Common.BaseResponse;
using Ichiba.Shipment.Application.Products.Commands;
using Ichiba.Shipment.Application.Products.Helper;
using Ichiba.Shipment.Domain.Consts;
using Ichiba.Shipment.Domain.Entities;
using Ichiba.Shipment.Infrastructure.Data;
using Ichiba.Shipment.Infrastructure.Services.Customers;
using Ichiba.Shipment.Infrastructure.Services.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ichiba.Shipment.Application.Packages.Commands;

public class CreateMultiplePackagesCommand : IRequest<BaseEntity<List<CreatePackageCommandResponse>>>
{
    public required Guid CustomerId { get; set; }
    public Guid WarehouseId { get; set; }
    public Guid CarrierId { get; set; }
    public List<PackageInfo> Packages { get; set; }
}

public class PackageInfo
{
    public string? PackageNumber { get; set; }
    public string? Note { get; set; }
    public PackageStatus Status { get; set; }
    public decimal Length { get; set; }
    public decimal Width { get; set; }
    public decimal Height { get; set; }
    public decimal Weight { get; set; }
    public Guid? CreateBy { get; set; }
    public List<CreateProductCommand>? Products { get; set; }
    public List<PKProductCreateDto>? PackageProducts { get; set; }
}

public class CreateMultiplePackagesCommandHandler : IRequestHandler<CreateMultiplePackagesCommand, BaseEntity<List<CreatePackageCommandResponse>>>
{
    private readonly ShipmentDbContext _dbContext;
    private readonly ICustomerService _customerService;
    private readonly IMediator _mediatR;

    public CreateMultiplePackagesCommandHandler(ShipmentDbContext dbContext, ICustomerService customerService, IMediator mediatR)
    {
        _dbContext = dbContext;
        _customerService = customerService;
        _mediatR = mediatR;
    }

    public async Task<BaseEntity<List<CreatePackageCommandResponse>>> Handle(CreateMultiplePackagesCommand request, CancellationToken cancellationToken)
    {
        using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        var responses = new List<CreatePackageCommandResponse>(); // Danh sách response cho mỗi package

        try
        {
            foreach (var packageInfo in request.Packages)
            {
                // Kiểm tra và tạo từng package
                var validationResult = await ValidateEntities(packageInfo, request.WarehouseId, request.CarrierId, request.CustomerId, cancellationToken);
                if (validationResult != null) return validationResult;

                var existCustomer = await _customerService.GetDetailCustomer(request.CustomerId);
                var defaultAddressCus = await GetCustomerDefaultAddress(request.CustomerId, cancellationToken);
                var warehouse = await _dbContext.Warehouses.AsNoTracking().SingleOrDefaultAsync(w => w.Id == request.WarehouseId, cancellationToken);

                var package = new Package
                {
                    Id = Guid.NewGuid(),
                    CustomerId = request.CustomerId,
                    WarehouseId = request.WarehouseId,
                    PackageNumber = await PackageNumberGenerator.GeneratePackageNumber(_dbContext,cancellationToken),
                    Note = packageInfo.Note,
                    Status = packageInfo.Status,
                    Length = packageInfo.Length,
                    Width = packageInfo.Width,
                    Height = packageInfo.Height,
                    Weight = packageInfo.Weight,
                    CreateAt = DateTime.UtcNow,
                    CreateBy = packageInfo.CreateBy,
                    CarrierId = request.CarrierId,
                    PackageAdresses = new List<PackageAddress>
                    {
                        new PackageAddress
                        {
                            Name = "Warehouse",
                            Phone = warehouse.Phone,
                            Address = warehouse.Address,
                            City = warehouse.City,
                            District = warehouse.District,
                            Ward = warehouse.Ward,
                            PostCode = warehouse.PostCode,
                            Type = ShipmentAddressType.SenderAddress
                        },
                        new PackageAddress
                        {
                            Name = "User",
                            Phone = existCustomer.PhoneNumber,
                            Address = defaultAddressCus.Address,
                            City = defaultAddressCus.City,
                            District = defaultAddressCus.District,
                            Ward = defaultAddressCus.Ward,
                            Type = ShipmentAddressType.ReceiveAddress
                        }
                    }
                };

                await _dbContext.Packages.AddAsync(package, cancellationToken);
                await _dbContext.SaveChangesAsync(cancellationToken);

                // **Tạo danh sách Product khi có đơn mua trực tiếp**
                if (packageInfo.Products?.Count > 0)
                {
                    var productId = Guid.NewGuid();
                    var products = packageInfo.Products.Select(p => new Product
                    {
                        Id = productId,
                        Name = p.Name,
                        Brand = p.Brand,
                        Category = p.Category,
                        Code = p.Code,
                        SKU = p.SKU,
                        ImageUrl = p.ImageUrl,
                        Description = p.Description,
                        MetaTitle = p.MetaTitle,
                        IsHasVariant = p.IsHasVariant,
                        Price = p.Price,
                        CreateAt = DateTime.UtcNow,
                        ProductAttributes = p.Attributes.Select(a => new ProductAttribute
                        {
                            ProductId = productId,
                            Id = Guid.NewGuid(),
                            Name = a.Name,
                            Values = a.Values.Select(v => new ProductAttributeValue
                            {
                                Id = Guid.NewGuid(),
                                Value = v
                            }).ToList()
                        }).ToList(),
                        Variants = p.Variants.Select(v => new ProductVariant
                        {
                            Id = Guid.NewGuid(),
                            SKU = v.SKU,
                            Price = v.Price,
                            Weight = v.Weight,
                            Length = v.Length,
                            Width = v.Width,
                            Height = v.Height,
                            StockQty = v.StockQty,
                            Images = v.ImageUrls.Select(i => new ProductVariantImage { ImageUrl = i }).ToList()
                        }).ToList()
                    }).ToList();

                    await _dbContext.Products.AddRangeAsync(products, cancellationToken);
                    await _dbContext.SaveChangesAsync(cancellationToken);

                    // **Tạo danh sách PackageProduct**
                    var packageProducts = products.Select(p => new PackageProduct
                    {
                        Id = Guid.NewGuid(),
                        ProductId = p.Id,
                        ProductName = p.Name,
                        PackageId = package.Id,
                        Quantity = (int)packageInfo.Products.First(pr => pr.Name == p.Name).Variants.Sum(v => v.StockQty),
                        Total = (double)(p.Variants.Sum(v => v.Price * v.StockQty))
                    }).ToList();

                    await _dbContext.PackageProducts.AddRangeAsync(packageProducts, cancellationToken);
                    await _dbContext.SaveChangesAsync(cancellationToken);
                }

                // **Nếu không có sản phẩm thì tạo từ PackageProducts**
                if (packageInfo.Products == null || packageInfo.Products.Count == 0)
                {
                    var packageProducts = packageInfo.PackageProducts.Select(pkd => new PackageProduct()
                    {
                        Id = Guid.NewGuid(),
                        //ProductId = Guid.NewGuid(),
                        ProductName = pkd.ProductName,
                        PackageId = package.Id,
                        Origin = pkd.Origin,
                        Unit = pkd.Unit,
                        ProductLink = pkd.ProductLink,
                        Tax = pkd.Tax,
                        OriginPrice = pkd.OriginPrice,
                        Quantity = pkd.Quantity,
                        Total = pkd.Quantity * pkd.OriginPrice
                    }).ToList();

                    await _dbContext.PackageProducts.AddRangeAsync(packageProducts, cancellationToken);
                    await _dbContext.SaveChangesAsync(cancellationToken);
                }

                // Thêm response cho từng package
                responses.Add(new CreatePackageCommandResponse { Id = package.Id });
            }

            await transaction.CommitAsync(cancellationToken);

            return new BaseEntity<List<CreatePackageCommandResponse>>
            {
                Status = true,
                Message = "Packages created successfully.",
                Data = responses  // Trả về danh sách response cho tất cả package
            };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            return new BaseEntity<List<CreatePackageCommandResponse>>
            {
                Status = false,
                Message = $"Error: {ex.Message}"
            };
        }
    }

    private async Task<BaseEntity<List<CreatePackageCommandResponse>>> ValidateEntities(PackageInfo packageInfo, Guid WarehouseId, Guid CarrierId, Guid CustomerId, CancellationToken cancellationToken)
    {
        var existWarehouse = await _dbContext.Warehouses.AsNoTracking().FirstOrDefaultAsync(w => w.Id == WarehouseId, cancellationToken);
        if (existWarehouse == null)
        {
            return new BaseEntity<List<CreatePackageCommandResponse>> { Message = "Not found Warehouse", Status = false };
        }

        var existCarrier = await _dbContext.Carriers.AsNoTracking().FirstOrDefaultAsync(i => i.Id == CarrierId, cancellationToken);
        if (existCarrier == null)
        {
            return new BaseEntity<List<CreatePackageCommandResponse>> { Message = "Not found Carrier", Status = false };
        }

        var existCustomer = await _customerService.GetDetailCustomer(CustomerId);
        if (existCustomer == null)
        {
            return new BaseEntity<List<CreatePackageCommandResponse>> { Message = "Not found Customer", Status = false };
        }

        return null;
    }

    private async Task<CustomerAddressView> GetCustomerDefaultAddress(Guid customerId, CancellationToken cancellationToken)
    {
        var existCustomer = await _customerService.GetDetailCustomer(customerId);
        return existCustomer?.Addresses.FirstOrDefault(a => a.IsDefaultAddress);
    }
}
