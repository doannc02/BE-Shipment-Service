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

namespace Ichiba.Shipment.Application.Shipments.Commands;

public class CreateShipmentsResponse
{
    public List<Guid> ShipmentIds { get; set; } = new();
}

public class CreateMultiShipmentsCommand : IRequest<BaseEntity<CreateShipmentsResponse>>
{
    public required List<CreateShipmentCommand> Shipments { get; set; } = new();
}

//public class CreateShipmentsCommandHandler : IRequestHandler<CreateMultiShipmentsCommand, CreateShipmentsResponse>
//{
//    private readonly IShipmentRepository _shipmentRepository;
//    private readonly ILogger<CreateShipmentsCommandHandler> _logger;
//    private readonly ICustomerService _customerService;

//    public CreateShipmentsCommandHandler(
//        IShipmentRepository shipmentRepository,
//        ILogger<CreateShipmentsCommandHandler> logger,
//        ICustomerService customerService)
//    {
//        _shipmentRepository = shipmentRepository;
//        _logger = logger;
//        _customerService = customerService;
//    }

//    public async Task<CreateShipmentsResponse> Handle(CreateMultiShipmentsCommand request, CancellationToken cancellationToken)
//    {
//        var shipmentIds = new List<Guid>();

//        foreach (var shipmentCommand in request.Shipments)
//        {
//            var IdGenerate = Guid.NewGuid();
//            string shipmentNumber;

//            do
//            {
//                shipmentNumber = await GenShipmentNumber(IdGenerate);
//            }
//            while (await _shipmentRepository.ShipmentNumberExistsAsync(shipmentNumber));

//            var currentCustomer = await GetCustomerById(shipmentCommand.CustomerId);
//            if (currentCustomer == null || currentCustomer.Id == Guid.Empty)
//            {
//                _logger.LogInformation($"Customer info is missing.");
//                throw new ArgumentNullException(nameof(currentCustomer), "Customer information is missing.");
//            }

//            var shipment = new ShipmentEntity()
//            {
//                Id = IdGenerate,
//                CustomerId = shipmentCommand.CustomerId,
//                ShipmentNumber = shipmentNumber,
//                CreateAt = DateTime.UtcNow,
//                Note = shipmentCommand.Note,
//                Status = ShipmentStatus.ShipmentCreated,
//                //Addresses = shipmentCommand.Addresses.Select(addr => new ShipmentAddress
//                //{
//                //    Id = Guid.NewGuid(),
//                //    ShipmentId = IdGenerate,
//                //    Type = ShipmentAddressType.SenderAddress,
//                //    Address = addr.Address,
//                //    City = addr.City,
//                //    Code = addr.Code,
//                //    District = addr.District,
//                //    Name = addr.Name,
//                //    CreateAt = DateTime.UtcNow
//                //}).ToList()
//            };

//            await _shipmentRepository.AddAsync(shipment);
//            shipmentIds.Add(shipment.Id);

//            _logger.LogInformation($"Shipment {shipmentNumber} created successfully.");
//        }

//        return new CreateShipmentsResponse
//        {
//            ShipmentIds = shipmentIds
//        };
//    }

//    private async Task<string> GenShipmentNumber(Guid guid)
//    {
//        string prefix = "SJA";
//        string datePart = DateTime.UtcNow.ToString("yyMM");
//        string valueSignature = guid.ToString("N").Substring(0, 4);

//        return $"{prefix}{datePart}{valueSignature}";
//    }

//    public async Task<CustomerEntityView> GetCustomerById(Guid idCustomer)
//    {
//        return await _customerService.GetDetailCustomer(idCustomer);
//    }
//}

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

        foreach (var shipmentRequest in request.Shipments)
        {
            var shipmentId = Guid.NewGuid();
            var shipmentNumber = await GenerateIdShipment.GenShipmentNumber(shipmentId);

            // Validate required entities
            var validationResult = await ValidateEntities(shipmentRequest, cancellationToken);
            if (validationResult != null)
            {
                continue;  // Skip this shipment if there's an error
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
                continue; // Skip this shipment if no valid packages found
            }

            var totalHeight = packages.Sum(pkg => pkg.Height);
            var totalWeight = packages.Sum(pkg => pkg.Weight);

            var shipmentAddresses = await GetShipmentAddressesFromPackages(packages, shipmentId);

            var shipmentPackages = packages.Select(pkg => new ShipmentPackage
            {
                Id = Guid.NewGuid(),
                ShipmentId = shipmentId,
                PackageId = pkg.Id,
                CreateAt = DateTime.UtcNow
            }).ToList();

            // Create the shipment
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
            };

            await _shipmentRepository.AddAsync(shipment);
            shipmentIds.Add(shipment.Id); // Collect the ID of the created shipment
            _logger.LogInformation($"Shipment {shipmentNumber} created successfully.");
        }

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
            var existingPackage = await _dbContext.Packages
                .FirstOrDefaultAsync(p => p.Id == incomingPackage.Id && p.CustomerId == customer.Id);
            var packageNumber = await GeneratePackageNumberAsync();
            if (existingPackage == null)
            {
                var newPackage = new Package
                {
                    Id = incomingPackage.Id,
                    PackageNumber = packageNumber, 
                    CustomerId = customer.Id,
                    PackageAdresses = packageAddresses,
                    CreateAt = DateTime.UtcNow,
                    Height = incomingPackage.Height,
                    CarrierId = incomingPackage.CarrierId,
                    CreateBy = incomingPackage.CreateBy,
                    Length = incomingPackage.Length,
                    Note = incomingPackage.Note,
                    WarehouseId =  warehouse.Id,
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

