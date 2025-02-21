using Ichiba.Shipment.Application.Common.BaseResponse;
using Ichiba.Shipment.Application.Common.Mappings;
using Ichiba.Shipment.Domain.Consts;
using Ichiba.Shipment.Domain.Entities;
using Ichiba.Shipment.Infrastructure.Data;
using Ichiba.Shipment.Infrastructure.Services.Customers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Net;

namespace Ichiba.Shipment.Application.Shipments.Queries
{
    public class GetShipmentDetailQuery : IRequest<BaseEntity<GetShipmentDetailQueryResponse>>
    {
        public Guid ShipmentId { get; set; }
    }

    public class GetShipmentDetailQueryResponse
    {
        public Guid Id { get; set; }
        public string ShipmentNumber { get; set; } = string.Empty;
        public Customer Customer { get; set; }
        public string? Note { get; set; }
        public ShipmentStatus Status { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal Weight { get; set; }
        public DateTime CreateAt { get; set; }
        public ShipmentAddressSMDto AddressSender { get; set; }
        public ShipmentAddressSMDto AddressReceive { get; set; }
        public List<PackageSMDto> Packages { get; set; } = new List<PackageSMDto>();
        public CarrierSMView Carrier { get; set; }
    }

    public class PackageSMDto
    {
        public Guid CustomerId { get; set; }
        // public virtual List<PackageAddressCreateSMDTO> PackageAddresses {get; set;}
        public PackageAddressCreateSMDTO AddressSender { get; set; }
        public PackageAddressCreateSMDTO AddressReceive { get; set; }
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid WarehouseId { get; set; }
        public string PackageNumber { get; set; } = string.Empty;
        public string? Note { get; set; }
        public PackageStatus Status { get; set; }
        public decimal Length { get; set; }
        public decimal Width { get; set; }
        public decimal Height { get; set; }
        public decimal Weight { get; set; }
    }
    public record ShipmentAddressSMDto
    {
        public Guid Id { get; set; }
        public Guid ShipmentId { get; set; }
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
        public ShipmentAddressType Type { get; set; }
        public DateTime CreateAt { get; set; } = DateTime.UtcNow;

    }

    public class GetShipmentDetailQueryHandler : IRequestHandler<GetShipmentDetailQuery, BaseEntity<GetShipmentDetailQueryResponse>>
    {
        private readonly ShipmentDbContext _dbContext;
        private readonly ICustomerService _customerService;
        private readonly ILogger<GetShipmentDetailQueryHandler> _logger;

        public GetShipmentDetailQueryHandler(
            ShipmentDbContext dbContext,
            ICustomerService customerService,
            ILogger<GetShipmentDetailQueryHandler> logger)
        {
            _dbContext = dbContext;
            _customerService = customerService;
            _logger = logger;
        }

        public async Task<BaseEntity<GetShipmentDetailQueryResponse>> Handle(GetShipmentDetailQuery request, CancellationToken cancellationToken)
        {
            try
            {
                // Truy vấn chi tiết Shipment từ DB, chỉ gọi một lần và lấy các địa chỉ vào bộ nhớ
                var shipment = await _dbContext.Shipments
                    .AsNoTracking()
                    .Include(s => s.Addresses)
                    .Include(s => s.ShipmentPackages)
                        .ThenInclude(sp => sp.Package)
                        .ThenInclude(pkg => pkg.PackageAdresses)
                    .FirstOrDefaultAsync(s => s.Id == request.ShipmentId, cancellationToken);

                if (shipment == null)
                {
                    return new BaseEntity<GetShipmentDetailQueryResponse>
                    {
                        Status = false,
                        Message = "Shipment not found",
                        Data = null
                    };
                }

                // Lấy thông tin khách hàng
                var customer = await _customerService.GetDetailCustomer(shipment.CustomerId);

                // Lấy thông tin Carrier từ bảng Carrier
                var carrier = await _dbContext.Carriers
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id == shipment.CarrierId, cancellationToken);

                // Tách địa chỉ Sender và Receive chỉ trong một lần truy vấn
                var addressSender = shipment.Addresses.FirstOrDefault(addr => addr.Type == ShipmentAddressType.SenderAddress);
                var addressReceive = shipment.Addresses.FirstOrDefault(addr => addr.Type == ShipmentAddressType.ReceiveAddress);

                // Chuẩn bị dữ liệu response
                var response = new GetShipmentDetailQueryResponse
                {
                    Id = shipment.Id,
                    ShipmentNumber = shipment.ShipmentNumber,
                    Status = shipment.Status,
                    TotalAmount = shipment.TotalAmount,
                    Weight = shipment.Weight,
                    CreateAt = shipment.CreateAt,
                    Customer = customer != null ? new Customer
                    {
                        Id = customer.Id,
                        Name = customer.FullName
                    } : null!,

                    // Set địa chỉ nhận hàng (AddressReceive)
                    AddressReceive = ShipmentAddressMapping.MapShipmentAddress(addressReceive),
                    // Set địa chỉ gửi hàng (AddressSender)
                    AddressSender = ShipmentAddressMapping.MapShipmentAddress(addressSender),
                    // Chuẩn bị danh sách các Package
                    Packages = shipment.ShipmentPackages.Select(sp => new PackageSMDto
                    {
                        Id = sp.Package.Id,
                        PackageNumber = sp.Package.PackageNumber,
                        Note = sp.Package.Note,
                        Status = sp.Package.Status,
                        Weight = sp.Package.Weight,
                        Width = sp.Package.Width,
                        Height = sp.Package.Height,
                        Length = sp.Package.Length,
                        WarehouseId = sp.Package.WarehouseId,
                        CustomerId = sp.Package.CustomerId,
                        AddressSender = PackageAddressMapping.MapPackageAddress(sp.Package.PackageAdresses.FirstOrDefault(i => i.Type == ShipmentAddressType.SenderAddress)),
                        AddressReceive = PackageAddressMapping.MapPackageAddress(sp.Package.PackageAdresses.FirstOrDefault(i => i.Type == ShipmentAddressType.ReceiveAddress)),
                    }).ToList(),

                    // Lấy thông tin Carrier từ dữ liệu đã truy vấn
                    Carrier = carrier != null ? new CarrierSMView
                    {
                        Id = carrier.Id,
                        Code = carrier.Code,
                        lastmile_tracking = carrier.lastmile_tracking,
                        logo = carrier.logo,
                        ShippingMethod = carrier.ShippingMethod,
                        Type = carrier.Type
                    } : null!
                };

                return new BaseEntity<GetShipmentDetailQueryResponse>
                {
                    Status = true,
                    Message = "Shipment details retrieved successfully.",
                    Data = response
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching shipment detail.");
                return new BaseEntity<GetShipmentDetailQueryResponse>
                {
                    Status = false,
                    Message = "An error occurred while fetching shipment detail.",
                    Data = null
                };
            }
        }
    }
}
