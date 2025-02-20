using Ichiba.Shipment.Application.Common.BaseResponse;
using Ichiba.Shipment.Application.Shipments.Queries;
using Ichiba.Shipment.Domain.Entities;
using Ichiba.Shipment.Infrastructure.Data;
using Ichiba.Shipment.Infrastructure.Services.Customers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

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
        public List<ShipmentAddressSMDto> Addresses { get; set; } = new List<ShipmentAddressSMDto>();
        public List<PackageSMDto> Packages { get; set; } = new List<PackageSMDto>();
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
                // Truy vấn chi tiết Shipment từ DB
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
                    Addresses = shipment.Addresses.Select(a => new ShipmentAddressSMDto
                    {
                        Id = a.Id,
                        ShipmentId = a.ShipmentId,
                        Name = a.Name,
                        Phone = a.Phone,
                        PhoneNumber = a.PhoneNumber,
                        PostCode = a.PostCode,
                        PrefixPhone = a.PrefixPhone,
                        Ward = a.Ward,
                        District = a.District,
                        City = a.City,
                        Address = a.Address,
                        Code = a.Code,
                        Type = a.Type,
                        CreateAt = a.CreateAt
                    }).ToList(),
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
                        PackageAddresses = sp.Package.PackageAdresses.Select(pa => new PackageAddressCreateSMDTO
                        {
                            Id = pa.Id,
                            PackageId = pa.PackageId,
                            Name = pa.Name,
                            Address = pa.Address,
                            City = pa.City,
                            District = pa.District,
                            Ward = pa.Ward,
                            Status = pa.Status,
                            Phone = pa.Phone,
                            PrefixPhone = pa.PrefixPhone,
                            PostCode = pa.PostCode,
                            UpdatedAt = DateTime.UtcNow
                        }).ToList()
                    }).ToList()
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
