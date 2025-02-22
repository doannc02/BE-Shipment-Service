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
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

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
    public decimal Weight { get; set; }
    public DateTime CreateAt { get; set; } = DateTime.UtcNow;
    public Guid? CreateBy { get; set; }
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
    public CreateShipmentCommandHandler(
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

    public async Task<BaseEntity<CreateShipmentResponse>> Handle(CreateShipmentCommand request, CancellationToken cancellationToken)
    {
        var shipmentId = Guid.NewGuid();

        var shipmentNumber = await GenerateIdShipment.GenShipmentNumber(shipmentId);

        var validationResult = await ValidateEntities(request, cancellationToken);
        if (validationResult != null) return validationResult;

        var customer = await _customerService.GetDetailCustomer(request.CustomerId);
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

        var packages = await ProcessPackages(request.Packages, customer, warehouse, defaultAddress);

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
            CreateAt = DateTime.UtcNow
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

        return new BaseEntity<CreateShipmentResponse>
        {
            Status = true,
            Data = new CreateShipmentResponse { Id = shipment.Id },
            Message = "Create successfully"
        };
    }

    private async Task<List<Package>> ProcessPackages(List<PackageCreateSM> incomingPackages, CustomerEntityView customer, Warehouse warehouse, CustomerAddressView defaultAddress)
    {
        var packages = new List<Package>();
        var packageAddresses = CreatePackageAddresses(warehouse, defaultAddress, customer);

        foreach (var incomingPackage in incomingPackages)
        {
            var existingPackage = await _dbContext.Packages
                .FirstOrDefaultAsync(p => p.Id == incomingPackage.Id && p.CustomerId == customer.Id);

            if (existingPackage == null)
            {
                var packageNumber = await GeneratePackageNumberAsync();

                var newPackage = new Package
                {
                    PackageNumber = packageNumber,
                    Id = incomingPackage.Id,
                    Height = incomingPackage.Height,
                    WarehouseId = warehouse.Id,
                    Weight = incomingPackage.Weight,
                    Width = incomingPackage.Width,
                    Length = incomingPackage.Length,
                    Note = incomingPackage.Note,
                    CustomerId = customer.Id,
                    PackageAdresses = packageAddresses,
                    CreateAt = DateTime.UtcNow,
                    CarrierId = incomingPackage.CarrierId,
                    CreateBy = incomingPackage.CreateBy,
                };

                await _dbContext.Packages.AddAsync(newPackage);
                packages.Add(newPackage);
            }
            else
            {
                packages.Add(existingPackage);
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
        var customer = await _customerService.GetDetailCustomer(customerId);
        return customer?.Addresses.FirstOrDefault(a => a.IsDefaultAddress);
    }

    private BaseEntity<CreateShipmentResponse> ErrorResponse(string message)
    {
        return new BaseEntity<CreateShipmentResponse> { Status = false, Message = message };
    }
}
