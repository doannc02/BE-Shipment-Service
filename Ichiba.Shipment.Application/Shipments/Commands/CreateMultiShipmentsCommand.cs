using Ichiba.Shipment.Application.Common.BaseResponse;
using Ichiba.Shipment.Application.Shipments.Helper;
using Ichiba.Shipment.Domain.Consts;
using Ichiba.Shipment.Domain.Entities;
using Ichiba.Shipment.Domain.Interfaces;
using Ichiba.Shipment.Infrastructure.Data;
using Ichiba.Shipment.Infrastructure.Services.Customers;
using Ichiba.Shipment.Infrastructure.Services.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Net.WebSockets;
using System.Threading;

namespace Ichiba.Shipment.Application.Shipments.Commands;

public class CreateShipmentsResponse
{
    public List<Guid> ShipmentIds { get; set; } = new();
}

public class CreateMultiShipmentsCommand : IRequest<BaseEntity<CreateShipmentsResponse>>
{
    public required List<CreateShipmentCommand> Shipments { get; set; } = new();
}

public class CreateMultiShipmentsCommandHandler : IRequestHandler<CreateMultiShipmentsCommand, BaseEntity<CreateShipmentsResponse>>
{
    private readonly IShipmentRepository _shipmentRepository;
    private readonly ILogger<CreateShipmentCommandHandler> _logger;
    private readonly ICustomerService _customerService;
    private readonly ShipmentDbContext _dbContext;

    public CreateMultiShipmentsCommandHandler(
        IShipmentRepository shipmentRepository,
        ILogger<CreateShipmentCommandHandler> logger,
        ICustomerService customerService,
        ShipmentDbContext dbContext)
    {
        _shipmentRepository = shipmentRepository;
        _logger = logger;
        _customerService = customerService;
        _dbContext = dbContext;
    }

    public async Task<BaseEntity<CreateShipmentsResponse>> Handle(CreateMultiShipmentsCommand request, CancellationToken cancellationToken)
    {
        var shipmentIds = new List<Guid>();

        var allPackagesToAdd = new List<Package>();
      //  var allPackageProductsToAdd = new List<PackageProduct>();
        var allShipmentPackagesToAdd = new List<ShipmentPackage>();
        var allShipmentsToAdd = new List<ShipmentEntity>();
        foreach (var shipmentRequest in request.Shipments)
        {
            var shipmentId = Guid.NewGuid();
            var shipmentNumber = await GenerateIdShipment.GenShipmentNumber(shipmentId);

            // Validate required entities
            var validationResult = await ValidateEntities(shipmentRequest, cancellationToken);
            if (validationResult != null)
            {
                _logger.LogError($"Shipment validation failed for {shipmentNumber}: {validationResult.Message}");
                continue;
            }

            var customer = await _customerService.GetDetailCustomer(shipmentRequest.CustomerId);
            if (customer == null)
            {
                _logger.LogError($"Customer not found: {shipmentRequest.CustomerId}");
                continue; // Skip this shipment if customer is not found
            }

            var defaultAddress = await GetCustomerDefaultAddress(shipmentRequest.CustomerId, cancellationToken);
            if (defaultAddress == null)
            {
                _logger.LogError($"Customer {shipmentRequest.CustomerId} does not have a default address.");
                continue; // Skip this shipment if no default address found
            }

            var warehouse = await _dbContext.Warehouses.AsNoTracking()
                .FirstOrDefaultAsync(w => w.Id == shipmentRequest.WarehouseId, cancellationToken);
            if (warehouse == null)
            {
                _logger.LogError($"Warehouse not found: {shipmentRequest.WarehouseId}");
                continue; // Skip this shipment if warehouse is not found
            }

            var carrier = await _dbContext.Carriers
                .FirstOrDefaultAsync(i => i.Id == shipmentRequest.CarrierId, cancellationToken);
            if (carrier == null)
            {
                _logger.LogError($"Carrier not found: {shipmentRequest.CarrierId}");
                continue; // Skip this shipment if carrier is not found
            }

            // Process packages (either existing or new)
            var packages = await ProcessPackages(shipmentRequest.Packages, customer, warehouse, defaultAddress);
            if (packages.Count == 0)
            {
                _logger.LogError($"No valid packages found for shipment {shipmentNumber}");
                continue;
            }

            var totalHeight = packages.Sum(pkg => pkg.Height);
            var totalWeight = packages.Sum(pkg => pkg.Weight);
            var totalAmount = packages.Sum(pkg => pkg.Amount);

            var shipmentAddresses = await GetShipmentAddressesFromPackages(packages, shipmentId);

            var shipmentPackages = packages.Select(pkg => new ShipmentPackage
            {
                Id = Guid.NewGuid(),
                ShipmentId = shipmentId,
                CreateBy = pkg.CreateBy,
                PackageId = pkg.Id,
                CreateAt = DateTime.UtcNow
            }).ToList();

            var shipment = new ShipmentEntity
            {
                Id = shipmentId,
                CustomerId = shipmentRequest.CustomerId,
                CarrierId = shipmentRequest.CarrierId,
                WarehouseId = shipmentRequest.WarehouseId,
                ShipmentNumber = shipmentNumber,
                CreateAt = DateTime.UtcNow,
                Note = shipmentRequest.Note,
                Status = ShipmentStatus.ShipmentCreated,
                Addresses = shipmentAddresses,
                ShipmentPackages = shipmentPackages,
                Height = totalHeight,
                Weight = totalWeight,
                TotalAmount = totalAmount,
                WeightUnit = shipmentRequest.WeightUnit,
                CubitUnit = shipmentRequest.CubitUnit
            };

            // Add to batch collections
            allShipmentsToAdd.Add(shipment);
            allPackagesToAdd.AddRange(packages);
            allShipmentPackagesToAdd.AddRange(shipmentPackages);
            shipmentIds.Add(shipment.Id);
            _logger.LogInformation($"Shipment {shipmentNumber} created successfully.");
        }

        // Save all entities in a batch
        await _dbContext.Packages.AddRangeAsync(allPackagesToAdd, cancellationToken);
      //  await _dbContext.PackageProducts.AddRangeAsync(allPackageProductsToAdd);
        await _dbContext.ShipmentPackages.AddRangeAsync(allShipmentPackagesToAdd, cancellationToken);
        await _dbContext.Shipments.AddRangeAsync(allShipmentsToAdd, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new BaseEntity<CreateShipmentsResponse>
        {
            Status = true,
            Data = new CreateShipmentsResponse { ShipmentIds = shipmentIds },
            Message = "Shipments created successfully"
        };
    }


    private async Task<List<Package>> ProcessPackages(List<PackageCreateSM> incomingPackages, CustomerEntityView customer, Warehouse warehouse, CustomerAddressView defaultAddress)
    {
        var packages = new List<Package>();
        var packageAddresses = CreatePackageAddresses(warehouse, defaultAddress, customer);

        foreach (var incomingPackage in incomingPackages)
        {
            var packageId = Guid.NewGuid();
            var existingPackage = await _dbContext.Packages
                .FirstOrDefaultAsync(p => p.Id == incomingPackage.Id && p.CustomerId == customer.Id);

            if (existingPackage == null)
            {
                var packageNumber = await GeneratePackageNumberAsync();
                var newPackage = new Package
                {
                    Id = packageId, 
                    PackageNumber = packageNumber,
                    CustomerId = customer.Id,
                    PackageAdresses = packageAddresses,
                    CreateAt = DateTime.UtcNow,
                    Height = incomingPackage.Height,
                    CarrierId = incomingPackage.CarrierId,
                    CreateBy = incomingPackage.CreateBy,
                    Length = incomingPackage.Length,
                    Note = incomingPackage.Note,
                    WarehouseId = warehouse.Id,
                    PackageProducts = new List<PackageProduct>()
                };

                packages.Add(newPackage);

                if (incomingPackage.PackageProducts != null && incomingPackage.PackageProducts.Any())
                {
                    var packageProducts = incomingPackage.PackageProducts.Select(pkd => new PackageProduct()
                    {
                        Id = Guid.NewGuid(),
                        ProductName = pkd.ProductName,
                        PackageId = packageId, 
                        Origin = pkd.Origin,
                        Unit = pkd.Unit,
                        ProductLink = pkd.ProductLink,
                        Tax = pkd.Tax,
                        OriginPrice = pkd.OriginPrice,
                        Quantity = pkd.Quantity,
                        Total = pkd.Quantity * pkd.OriginPrice
                    }).ToList();

                    newPackage.PackageProducts.AddRange(packageProducts);
                }
            }
            else
            {
                packages.Add(existingPackage);
                packageId = existingPackage.Id; 

                if (incomingPackage.PackageProducts != null && incomingPackage.PackageProducts.Any())
                {
                    var packageProducts = incomingPackage.PackageProducts.Select(pkd => new PackageProduct()
                    {
                        Id = Guid.NewGuid(),
                        ProductName = pkd.ProductName,
                        PackageId = packageId, 
                        Origin = pkd.Origin,
                        Unit = pkd.Unit,
                        ProductLink = pkd.ProductLink,
                        Tax = pkd.Tax,
                        OriginPrice = pkd.OriginPrice,
                        Quantity = pkd.Quantity,
                        Total = pkd.Quantity * pkd.OriginPrice
                    }).ToList();

                    existingPackage.PackageProducts.AddRange(packageProducts);
                }
            }

            if (incomingPackage.Products != null && incomingPackage.Products.Any())
            {
                var products = incomingPackage.Products.Select(p => new Product
                {
                    Id = Guid.NewGuid(),
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
                        Id = Guid.NewGuid(),
                        ProductId = Guid.NewGuid(),
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

                var packageProducts = products.Select(p => new PackageProduct
                {
                    Id = Guid.NewGuid(),
                    ProductId = p.Id,
                    ProductName = p.Name,
                    PackageId = packageId, 
                    Quantity = (int)incomingPackage.Products.First(pr => pr.Name == p.Name).Variants.Sum(v => v.StockQty),
                    Total = (double)(p.Variants.Sum(v => v.Price * v.StockQty))
                }).ToList();

                var newPackage = packages.FirstOrDefault(pkg => pkg.Id == packageId);
                newPackage?.PackageProducts.AddRange(packageProducts);
            }
        }

        return packages;
    }

    private async Task<string> GeneratePackageNumberAsync()
    {
        string datePart = DateTime.UtcNow.ToString("yyMMdd");

        int serialNumber = await _dbContext.Packages
            .Where(p => p.PackageNumber.StartsWith("PK" + datePart))
            .CountAsync();

        serialNumber++;

        string serialNumberPart = serialNumber.ToString("D4");

        string packageNumber = $"PK{datePart}{serialNumberPart}";

        return packageNumber;
    }
    private List<PackageAddress> CreatePackageAddresses(Warehouse warehouse, CustomerAddressView defaultAddress, CustomerEntityView customer)
    {
        return new List<PackageAddress>
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
                Phone = customer.PhoneNumber,
                Address = defaultAddress.Address,
                City = defaultAddress.City,
                District = defaultAddress.District,
                Ward = defaultAddress.Ward,
                Type = ShipmentAddressType.ReceiveAddress
            }
        };
    }

    private async Task<List<ShipmentAddress>> GetShipmentAddressesFromPackages(List<Package> packages, Guid shipmentId)
    {
        var shipmentAddresses = new List<ShipmentAddress>();

        foreach (var package in packages)
        {
            var senderAddress = package.PackageAdresses.FirstOrDefault(a => a.Type == ShipmentAddressType.SenderAddress);
            var receiverAddress = package.PackageAdresses.FirstOrDefault(a => a.Type == ShipmentAddressType.ReceiveAddress);

            AddShipmentAddress(shipmentAddresses, senderAddress, shipmentId, ShipmentAddressType.SenderAddress);
            AddShipmentAddress(shipmentAddresses, receiverAddress, shipmentId, ShipmentAddressType.ReceiveAddress);
        }

        return shipmentAddresses;
    }

    private void AddShipmentAddress(List<ShipmentAddress> shipmentAddresses, PackageAddress address, Guid shipmentId, ShipmentAddressType type)
    {
        if (address != null)
        {
            shipmentAddresses.Add(new ShipmentAddress
            {
                Id = Guid.NewGuid(),
                ShipmentId = shipmentId,
                Type = type,
                Address = address.Address,
                City = address.City,
                District = address.District,
                Ward = address.Ward,
                CreateAt = DateTime.UtcNow
            });
        }
    }
    private async Task<BaseEntity<CreateShipmentsResponse>> ValidateEntities(CreateShipmentCommand request, CancellationToken cancellationToken)
    {
        if (!await _dbContext.Warehouses.AnyAsync(w => w.Id == request.WarehouseId, cancellationToken))
            return ErrorResponse("Warehouse not found.");

        if (!await _dbContext.Carriers.AnyAsync(c => c.Id == request.CarrierId, cancellationToken))
            return ErrorResponse("Carrier not found.");

        if (await _customerService.GetDetailCustomer(request.CustomerId) == null)
            return ErrorResponse("Customer not found.");

        return null;
    }

    private async Task<CustomerAddressView> GetCustomerDefaultAddress(Guid customerId, CancellationToken cancellationToken)
    {
        var customer = await _customerService.GetDetailCustomer(customerId);
        return customer?.Addresses.FirstOrDefault(a => a.IsDefaultAddress);
    }

    private BaseEntity<CreateShipmentsResponse> ErrorResponse(string message)
    {
        return new BaseEntity<CreateShipmentsResponse> { Status = false, Message = message };
    }
}

