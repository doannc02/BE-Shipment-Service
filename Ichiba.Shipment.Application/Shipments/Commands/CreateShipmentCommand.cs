using AutoMapper;
using Ichiba.Shipment.Domain.Consts;
using Ichiba.Shipment.Domain.Entities;
using Ichiba.Shipment.Domain.Interfaces;
using Ichiba.Shipment.Infrastructure.Services.Customers;
using Ichiba.Shipment.Infrastructure.Services.Models;
using MediatR;
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
    public string Status { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public decimal Weight { get; set; }
    public List<ShipmentAddressDTO>? Addresses { get; set; } = new();
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

    public CreateShipmentCommandHandler(
        IShipmentRepository shipmentRepository,
        ILogger<CreateShipmentCommandHandler> logger,
        ICustomerService customerService)
    {
        _shipmentRepository = shipmentRepository;
        _logger = logger;
        _customerService = customerService;
    }

    public async Task<CreateShipmentResponse> Handle(CreateShipmentCommand request, CancellationToken cancellationToken)
    {
        var IdGenerate = Guid.NewGuid();

        string shipmentNumber;
        do
        {
            shipmentNumber = await GenShipmentNumber(IdGenerate);
        }
        while (await _shipmentRepository.ShipmentNumberExistsAsync(shipmentNumber));
        var currentCustomer = await GetCustomerById(request.CustomerId);
        if (currentCustomer == null || currentCustomer.Id == Guid.Empty)
        {
            throw new ArgumentNullException(nameof(currentCustomer), "Customer information is missing.");
        }
        var shipment = new ShipmentEntity()
        {
            Addresses = request.Addresses.Select(addr => new ShipmentAddress
            {
                Id = new Guid(),
                ShipmentId = IdGenerate,
                Type = ShipmentAddressType.SenderAddress,
                Address = addr.Address,
                City = addr.City,
                Code = addr.Code,
                District = addr.District,
                Name = addr.Name,
                CreateAt = DateTime.UtcNow
            }).ToList()
        };
        shipment.Id = IdGenerate;
        shipment.CustomerId = request.CustomerId;
        shipment.ShipmentNumber = shipmentNumber;
        shipment.CreateAt = DateTime.UtcNow;
        shipment.Note = request.Note;

        await _shipmentRepository.AddAsync(shipment);

        _logger.LogInformation($"Shipment {shipmentNumber} created successfully.");

        //return _mapper.Map<CreateShipmentResponse>(shipment);
        return new CreateShipmentResponse()
        {
            Id = shipment.Id
        };
    }

    private async Task<string> GenShipmentNumber(Guid guid)
    {
        string prefix = "SJA";
        string datePart = DateTime.UtcNow.ToString("yyMM");
        string valueSignature = guid.ToString("N").Substring(0, 4);

        return $"{prefix}{datePart}{valueSignature}";
    }

    public async Task<CustomerEntityView> GetCustomerById(Guid idCustomer)
    {
        return await _customerService.GetDetailCustomer(idCustomer);
    }
}





