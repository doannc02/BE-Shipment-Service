using Ichiba.Shipment.Domain.Consts;
using Ichiba.Shipment.Domain.Entities;
using Ichiba.Shipment.Domain.Interfaces;
using Ichiba.Shipment.Infrastructure.Services.Customers;
using Ichiba.Shipment.Infrastructure.Services.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Ichiba.Shipment.Application.Shipments.Commands;

public class CreateShipmentsResponse
{
    public List<Guid> ShipmentIds { get; set; } = new();
}

public class CreateMultiShipmentsCommand : IRequest<CreateShipmentsResponse>
{
    public required List<CreateShipmentCommand> Shipments { get; set; } = new();
}

public class CreateShipmentsCommandHandler : IRequestHandler<CreateMultiShipmentsCommand, CreateShipmentsResponse>
{
    private readonly IShipmentRepository _shipmentRepository;
    private readonly ILogger<CreateShipmentsCommandHandler> _logger;
    private readonly ICustomerService _customerService;

    public CreateShipmentsCommandHandler(
        IShipmentRepository shipmentRepository,
        ILogger<CreateShipmentsCommandHandler> logger,
        ICustomerService customerService)
    {
        _shipmentRepository = shipmentRepository;
        _logger = logger;
        _customerService = customerService;
    }

    public async Task<CreateShipmentsResponse> Handle(CreateMultiShipmentsCommand request, CancellationToken cancellationToken)
    {
        var shipmentIds = new List<Guid>();

        foreach (var shipmentCommand in request.Shipments)
        {
            var IdGenerate = Guid.NewGuid();
            string shipmentNumber;

            do
            {
                shipmentNumber = await GenShipmentNumber(IdGenerate);
            }
            while (await _shipmentRepository.ShipmentNumberExistsAsync(shipmentNumber));

            var currentCustomer = await GetCustomerById(shipmentCommand.CustomerId);
            if (currentCustomer == null || currentCustomer.Id == Guid.Empty)
            {
                _logger.LogInformation($"Customer info is missing.");
                throw new ArgumentNullException(nameof(currentCustomer), "Customer information is missing.");
            }

            var shipment = new ShipmentEntity()
            {
                Id = IdGenerate,
                CustomerId = shipmentCommand.CustomerId,
                ShipmentNumber = shipmentNumber,
                CreateAt = DateTime.UtcNow,
                Note = shipmentCommand.Note,
                Status = ShipmentStatus.ShipmentCreated,
                Addresses = shipmentCommand.Addresses.Select(addr => new ShipmentAddress
                {
                    Id = Guid.NewGuid(),
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

            await _shipmentRepository.AddAsync(shipment);
            shipmentIds.Add(shipment.Id);

            _logger.LogInformation($"Shipment {shipmentNumber} created successfully.");
        }

        return new CreateShipmentsResponse
        {
            ShipmentIds = shipmentIds
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
