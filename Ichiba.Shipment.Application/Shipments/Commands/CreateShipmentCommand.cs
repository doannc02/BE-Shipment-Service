using Dapr.Client;
using Ichiba.Shipment.Application.Common.BaseResponse;
using Ichiba.Shipment.Application.Packages.Commands;
using Ichiba.Shipment.Application.Products.Commands;
using Ichiba.Shipment.Application.Shipments.Helper;
using Ichiba.Shipment.Application.Shipments.Queries;
using Ichiba.Shipment.Domain.Consts;
using Ichiba.Shipment.Domain.Entities;
using Ichiba.Shipment.Domain.Interfaces;
using Ichiba.Shipment.Infrastructure.Connecter.CustomerService;
using Ichiba.Shipment.Infrastructure.Data;
using Ichiba.Shipment.Infrastructure.Services.Customers;
using Ichiba.Shipment.Infrastructure.Services.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using static Google.Rpc.Context.AttributeContext.Types;

namespace Ichiba.Shipment.Application.Shipments.Commands;
public class CreateShipmentResponse
{
    public Guid? Id { get; set; } = Guid.NewGuid();
}

public class CreateShipmentCommand : IRequest<BaseEntity<CreateShipmentResponse>>
{
    public Guid WarehouseId { get; set; }
    public Guid CustomerId { get; set; }
    public string? Note { get; set; }
    public ShipmentStatus Status { get; set; }
    public Guid CarrierId { get; set; }
    public CubitUnit CubitUnit { get; set; }
    public WeightUnit WeightUnit { get; set; }
    public List<PackageCreateSM> Packages { get; set; } = new();
}

public class PackageCreateSM
{
    public Guid CustomerId { get; set; }
    public Guid CarrierId { get; set; }
    public Guid Id { get; set; }
    public Guid WarehouseId { get; set; }
    public string PackageNumber { get; set; } = string.Empty;
    public string? Note { get; set; }
    public PackageStatus Status { get; set; }
    public decimal Length { get; set; }
    public decimal Width { get; set; }
    public decimal Height { get; set; }
    public decimal Amount { get; set; }
    public decimal Weight { get; set; }
    public CubitUnit CubitUnit { get; set; }
    public WeightUnit WeightUnit { get; set; }
    public DateTime CreateAt { get; set; } = DateTime.UtcNow;
    public Guid? CreateBy { get; set; } = Guid.NewGuid();
    public List<CreateProductCommand>? Products { get; set; }
    public List<PKProductCreateDto>? PackageProducts { get; set; }
}

public record ShipmentAddressDTO
{
    public string Name { get; set; } = string.Empty;
    public string? PrefixPhone { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Code { get; set; }
    public string? Phone { get; set; }
    public string? City { get; set; }
    public string? District { get; set; }
    public string? Ward { get; set; }
    public string? PostCode { get; set; }
    public string Address { get; set; } = string.Empty;

    [JsonConverter(typeof(StringEnumConverter))]
    public ShipmentAddressType Type { get; set; }
}

public class CreateShipmentCommandHandler : IRequestHandler<CreateShipmentCommand, BaseEntity<CreateShipmentResponse>>
{
    private readonly IShipmentRepository _shipmentRepository;
    private readonly ILogger<CreateShipmentCommandHandler> _logger;
    private readonly ICustomerService _customerService;
    private readonly ShipmentDbContext _dbContext;
    private readonly DaprCustomerService _customer;
    private readonly DaprClient _dapr;
    public CreateShipmentCommandHandler(
        IShipmentRepository shipmentRepository,
        ILogger<CreateShipmentCommandHandler> logger,
        ICustomerService customerService,
        ShipmentDbContext dbContext,
        DaprClient dapr, DaprCustomerService customer)
    {
        _shipmentRepository = shipmentRepository;
        _logger = logger;
        _customerService = customerService;
        _dbContext = dbContext;
        _dapr = dapr;
        _customer = customer;
    }

    public async Task<BaseEntity<CreateShipmentResponse>> Handle(CreateShipmentCommand request, CancellationToken cancellationToken)
    {
        var shipmentId = Guid.NewGuid();

        var shipmentNumber = await GenerateIdShipment.GenShipmentNumber(shipmentId);

        var validationResult = await ValidateEntities(request, cancellationToken);
        if (validationResult != null) return validationResult;

        //var customer = await _customerService.GetDetailCustomer(request.CustomerId);
       

        var customer = await _customer.CallCustomerServiceAsync(request.CustomerId.ToString());
        if (customer == null)
            return ErrorResponse("Customer not found.");
        var defaultAddress = await GetCustomerDefaultAddress(request.CustomerId, cancellationToken);
        if (defaultAddress == null)
            return ErrorResponse("Customer has not selected a default shipping address.");

        var warehouse = await _dbContext.Warehouses.AsNoTracking()
            .FirstOrDefaultAsync(w => w.Id == request.WarehouseId, cancellationToken);
        if (warehouse == null)
            return ErrorResponse("Warehouse not found.");

        var carrier = await _dbContext.Carriers
            .FirstOrDefaultAsync(i => i.Id == request.CarrierId, cancellationToken);
        if (carrier == null)
            return ErrorResponse("Carrier not found.");

        var packages = await ProcessPackages(request.Packages, customer, warehouse, defaultAddress, cancellationToken);

        if (packages.Count == 0)
            return ErrorResponse("No valid packages found.");

        var totalHeight = packages.Sum(pkg => pkg.Height);
        var totalWeight = packages.Sum(pkg => pkg.Weight);
        var totalAmount = packages.Sum(pkg => pkg.Amount);

        var shipmentAddresses = await GetShipmentAddressesFromPackages(packages, shipmentId);

        var shipmentPackages = packages.Select(pkg => new ShipmentPackage
        {
            Id = Guid.NewGuid(),
            ShipmentId = shipmentId,
            PackageId = pkg.Id,
            CreateAt = DateTime.UtcNow,
            CreateBy = pkg.CreateBy,
        }).ToList();

        var shipment = new ShipmentEntity
        {
            Id = shipmentId,
            CustomerId = request.CustomerId,
            CarrierId = carrier.Id,
            WarehouseId = warehouse.Id,
            ShipmentNumber = shipmentNumber,
            CreateAt = DateTime.UtcNow,
            Note = request.Note,
            Status = ShipmentStatus.ShipmentCreated,
            Addresses = shipmentAddresses,
            ShipmentPackages = shipmentPackages,
            Height = totalHeight,
            Weight = totalWeight,
            TotalAmount = totalAmount
        };

        await _shipmentRepository.AddAsync(shipment);
        _logger.LogInformation($"Shipment {shipmentNumber} created successfully.");

        await _dapr.PublishEventAsync("ichiba-customer-notification-pubsub", "Notification", new
        {
            ShipmentId = shipment.Id,
            ShipmentNumber = shipment.ShipmentNumber,
            CustomerId = shipment.CustomerId,
            Status = true
        });


        return new BaseEntity<CreateShipmentResponse>
        {
            Status = true,
            Data = new CreateShipmentResponse { Id = shipment.Id },
            Message = "Create successfully"
        };
    }

    private async Task<List<Package>> ProcessPackages(List<PackageCreateSM> incomingPackages, CustomerEntityView customer, Warehouse warehouse, CustomerAddressView defaultAddress, CancellationToken cancellationToken)
    {
        var packages = new List<Package>();
        var packageAddresses = CreatePackageAddresses(warehouse, defaultAddress, customer);

        // Lấy tất cả các package hiện có để tra cứu nhanh
        var existingPackageIds = incomingPackages.Select(ip => ip.Id).ToList();
        var existingPackages = await _dbContext.Packages
            .Where(p => existingPackageIds.Contains(p.Id) && p.CustomerId == customer.Id)
            .ToDictionaryAsync(p => p.Id, cancellationToken); // Tạo từ điển để tra cứu nhanh

        var productsToAdd = new List<Product>();
        var packageProductsToAdd = new List<PackageProduct>();

        // Lưu số package number trước, nếu có thể
        var packageNumbers = new Dictionary<Guid, string>();

        foreach (var incomingPackage in incomingPackages)
        {
            Package package = null;

            if (!existingPackages.TryGetValue(incomingPackage.Id, out package)) // Kiểm tra nếu package đã tồn tại
            {
                // Chỉ tạo mới nếu chưa có package trong cơ sở dữ liệu
                var packageNumber = await GeneratePackageNumberAsync();
                packageNumbers[incomingPackage.Id] = packageNumber;

                package = new Package
                {
                    PackageNumber = packageNumber,
                    Id = Guid.NewGuid(), // Sửa lỗi Id có thể bị trùng
                    Height = incomingPackage.Height,
                    WarehouseId = warehouse.Id,
                    Weight = incomingPackage.Weight,
                    Width = incomingPackage.Width,
                    Length = incomingPackage.Length,
                    Amount = incomingPackage.Amount,
                    CubitUnit = incomingPackage.CubitUnit,
                    WeightUnit = incomingPackage.WeightUnit,
                    Note = incomingPackage.Note,
                    CustomerId = customer.Id,
                    PackageAdresses = packageAddresses,
                    CreateAt = DateTime.UtcNow,
                    CarrierId = incomingPackage.CarrierId,
                    CreateBy = incomingPackage.CreateBy,
                };
                packages.Add(package);
            }

            // Tạo Product & PackageProduct
            if (incomingPackage.Products != null && incomingPackage.Products.Count > 0)
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
                }).ToList();

                productsToAdd.AddRange(products);

                var packageProducts = products.Select(p => new PackageProduct
                {
                    Id = Guid.NewGuid(),
                    ProductId = p.Id,
                    ProductName = p.Name,
                    PackageId = package.Id, // Đảm bảo PackageId tồn tại
                    Quantity = (int)incomingPackage.Products.First(pr => pr.Name == p.Name).Variants.Sum(v => v.StockQty),
                    Total = (double)(p.Variants.Sum(v => v.Price * v.StockQty))
                }).ToList();

                packageProductsToAdd.AddRange(packageProducts);
            }
            else
            {
                var packageProducts = incomingPackage.PackageProducts.Select(pkd => new PackageProduct()
                {
                    Id = Guid.NewGuid(),
                  
                    ProductName = pkd.ProductName,
                    PackageId = package.Id, // Đảm bảo PackageId tồn tại
                    Origin = pkd.Origin,
                    Unit = pkd.Unit,
                    ProductLink = pkd.ProductLink,
                    Tax = pkd.Tax,
                    OriginPrice = pkd.OriginPrice,
                    Quantity = pkd.Quantity,
                    Total = pkd.Quantity * pkd.OriginPrice
                }).ToList();

                packageProductsToAdd.AddRange(packageProducts);
            }
        }


        if (productsToAdd.Any())
        {
            await _dbContext.Products.AddRangeAsync(productsToAdd, cancellationToken);
        }

        if (packageProductsToAdd.Any())
        {
            await _dbContext.PackageProducts.AddRangeAsync(packageProductsToAdd, cancellationToken);
        }

        if (packages.Any())
        {
            await _dbContext.Packages.AddRangeAsync(packages, cancellationToken);
        }

        await _dbContext.SaveChangesAsync(cancellationToken); // Chỉ gọi SaveChanges một lần duy nhất

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

    private async Task<BaseEntity<CreateShipmentResponse>> ValidateEntities(CreateShipmentCommand request, CancellationToken cancellationToken)
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
       // var customer = await _customerService.GetDetailCustomer(customerId);
        var customer = await _customer.CallCustomerServiceAsync(customerId.ToString());
        return customer?.Addresses.FirstOrDefault(a => a.IsDefaultAddress);
    }

    private BaseEntity<CreateShipmentResponse> ErrorResponse(string message)
    {
        return new BaseEntity<CreateShipmentResponse> { Status = false, Message = message };
    }
}
