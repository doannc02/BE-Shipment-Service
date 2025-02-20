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

public class CreateShipmentCommand : IRequest<CreateShipmentResponse>
{
    public Guid WarehouseId { get; set; }
    public Guid CustomerId { get; set; }
    public string? Note { get; set; }
    public ShipmentStatus  Status { get; set; } 
    public decimal TotalAmount { get; set; }
    //public decimal Weight { get; set; }
    //public decimal Height { get; set; }
    //public List<ShipmentAddressDTO>? Addresses { get; set; } = new();
    public List<Packages> Packages { get; set; } = new();
}

public record Packages
{
    public Guid Id { get; set; }
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

public class CreateShipmentCommandHandler : IRequestHandler<CreateShipmentCommand, CreateShipmentResponse>
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

    public async Task<CreateShipmentResponse> Handle(CreateShipmentCommand request, CancellationToken cancellationToken)
    {
        var IdGenerate = Guid.NewGuid();

        // Sinh Shipment Number
        string shipmentNumber;
        do
        {
            shipmentNumber = await GenShipmentNumber(IdGenerate);
        }
        while (await _shipmentRepository.ShipmentNumberExistsAsync(shipmentNumber));

        // Lấy thông tin Customer
        var currentCustomer = await GetCustomerById(request.CustomerId);
        if (currentCustomer == null || currentCustomer.Id == Guid.Empty)
        {
            throw new ArgumentNullException(nameof(currentCustomer), "Customer information is missing.");
        }

        var packageIds = request.Packages.Select(p => p.Id).ToList();

        // Kiểm tra package có hợp lệ không
        var packages = await GetValidPackages(packageIds);

        // Tính tổng Height và Weight từ các Package
        var totalHeight = packages.Sum(pkg => pkg.Height); // Tổng chiều cao
        var totalWeight = packages.Sum(pkg => pkg.Weight); // Tổng trọng lượng

        // Tạo ShipmentAddresses từ Package hoặc Customer
        var shipmentAddresses = await GetShipmentAddresses(packages, currentCustomer, request.CustomerId, IdGenerate);

        // Tạo các bản ghi ShipmentPackage
        var shipmentPackages = packages.Select(pkg => new ShipmentPackage
        {
            Id = Guid.NewGuid(),
            ShipmentId = IdGenerate,
            PackageId = pkg.Id,
            CreateAt = DateTime.UtcNow
        }).ToList();

        // Tạo Shipment Entity
        var shipment = new ShipmentEntity()
        {
            Id = IdGenerate,
            CustomerId = request.CustomerId,
            WarehouseId = request.WarehouseId,
            ShipmentNumber = shipmentNumber,
            CreateAt = DateTime.UtcNow,
            Note = request.Note,
            Status = ShipmentStatus.ShipmentCreated,
            Addresses = shipmentAddresses,
            ShipmentPackages = shipmentPackages,
            Height = totalHeight, 
            Weight = totalWeight  
        };

        // Lưu Shipment vào DB
        await _shipmentRepository.AddAsync(shipment);
        _logger.LogInformation($"Shipment {shipmentNumber} created successfully.");

        return new CreateShipmentResponse()
        {
            Id = shipment.Id
        };
    }

    private async Task<List<Package>> GetValidPackages(List<Guid> packageIds)
    {
        var existingShipmentPackages = await _dbContext.ShipmentPackages
            .Where(sp => packageIds.Contains(sp.PackageId))
            .ToListAsync();

        if (existingShipmentPackages.Any())
        {
            throw new ArgumentException("Some packages are already assigned to another shipment.");
        }

        var packages = await _dbContext.Packages
            .Where(p => packageIds.Contains(p.Id) && p.DeleteAt == null).Include(i => i.PackageAdresses)
            .ToListAsync();

        if (!packages.Any())
        {
            throw new ArgumentException("No valid packages found.");
        }

        return packages;
    }

    private async Task<List<ShipmentAddress>> GetShipmentAddresses(List<Package> packages, CustomerEntityView currentCustomer, Guid customerId, Guid shipmentId)
    {
        List<ShipmentAddress> shipmentAddresses = new List<ShipmentAddress>();

        if (packages.Any())
        {
            // Kiểm tra nếu PackageAdresses không null
            shipmentAddresses.AddRange(packages
                .Where(pkg => pkg.PackageAdresses != null) // Kiểm tra PackageAdresses không phải null
                .SelectMany(pkg => pkg.PackageAdresses
                    .Where(addr => addr != null) // Kiểm tra nếu Address trong PackageAdresses không null
                    .Select(addr => new ShipmentAddress
                    {
                        Id = Guid.NewGuid(),
                        ShipmentId = shipmentId,
                        Type = addr.Type,
                        Address = addr.Address,
                        City = addr.City,
                        District = addr.District,
                        Ward = addr.Ward,
                        Code = addr.Code,
                        Name = addr.Name,
                        CreateAt = DateTime.UtcNow
                    })).ToList());
        }
        else
        {
            // Nếu không có package, lấy địa chỉ từ Customer
            shipmentAddresses.AddRange(currentCustomer.Addresses.Select(addr => new ShipmentAddress
            {
                Id = Guid.NewGuid(),
                ShipmentId = shipmentId,
                Type = ShipmentAddressType.ReceiveAddress,
                Address = addr.Address,
                City = addr.City,
                District = addr.District,
                Ward = addr.Ward,
                CreateAt = DateTime.UtcNow
            }).ToList());
        }

        return shipmentAddresses;
    }

    private async Task<string> GenShipmentNumber(Guid guid)
    {
        string prefix = "SJA";
        string datePart = DateTime.UtcNow.ToString("yyMM");
        string valueSignature = guid.ToString("N").Substring(0, 4);
        return $"{prefix}{datePart}{valueSignature}";
    }

    private async Task<CustomerEntityView> GetCustomerById(Guid idCustomer)
    {
        return await _customerService.GetDetailCustomer(idCustomer);
    }
}

