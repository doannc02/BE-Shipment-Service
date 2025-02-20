using Ichiba.Shipment.Application.Common.BaseRequest;
using Ichiba.Shipment.Application.Common.BaseResponse;
using Ichiba.Shipment.Application.Packages.Commands;
using Ichiba.Shipment.Domain.Consts;
using Ichiba.Shipment.Domain.Entities;
using Ichiba.Shipment.Infrastructure.Data;
using Ichiba.Shipment.Infrastructure.Services.Customers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Ichiba.Shipment.Application.Shipments.Queries;

public record Customer
{
    public Guid Id { get; set; }
    public string Name { get; set; }
}
public class GetListShipmentQueryResponse
{
    public Guid Id { get; set; }
    public Guid WarehouseId { get; set; }
    public Customer Customer { get; set; }
    public string ShipmentNumber { get; set; } = string.Empty;
    public string? Note { get; set; }
    public ShipmentStatus Status { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal Weight { get; set; }
    public DateTime CreateAt { get; set; } = DateTime.UtcNow;
    public Guid? CreateBy { get; set; }
    public DateTime? UpdateAt { get; set; }
    public Guid? UpdateBy { get; set; }
    public DateTime? DeleteAt { get; set; }
    public Guid? DeleteBy { get; set; }
    public virtual List<ShipmentAddressSMDto> Addresses { get; set; } = new();
    public virtual List<PackageSMDto> Packages { get; set; }
}

public class PackageSMDto
{
    public Guid CustomerId { get; set; }
    public virtual List<PackageAddressCreateSMDTO> PackageAddresses {get; set;}
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

public record ShipmentPackageDto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ShipmentId { get; set; }
    public Guid PackageId { get; set; }
    public DateTime CreateAt { get; set; } = DateTime.UtcNow;
    public Guid? CreateBy { get; set; }
}

public class PackageAddressCreateSMDTO
{
    public Guid Id { get; set; }
    public Guid PackageId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? PrefixPhone { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Code { get; set; }
    public string? Phone { get; set; }
    public string? City { get; set; }
    public string? District { get; set; }
    public string? Ward { get; set; }
    public string? PostCode { get; set; }
    public AddressStatus Status { get; set; }
    public string Address { get; set; } = string.Empty;
    public ShipmentAddressType Type { get; set; }
    public Guid? UpdatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? Country { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? DeliveryInstructions { get; set; }
    public bool IsDefault { get; set; } = false;
    public bool SensitiveDataFlag { get; set; } = false;
    public DateTime? EstimatedDeliveryDate { get; set; }
    public DateTime? DeliveryDate { get; set; }
    public string SearchIndex { get; set; } = string.Empty;
}
public class GetListShipmentQuery : QueryPage, IRequest<PageResponse<GetListShipmentQueryResponse>>
{
}

public class GetListShipmentQueryHandler : IRequestHandler<GetListShipmentQuery, PageResponse<GetListShipmentQueryResponse>>
{
    private readonly ShipmentDbContext _dbContext;
    private readonly ICustomerService _customerService;
    private readonly ICustomerBatchLookupService _customerBatchLookupService;
    private readonly ILogger<GetListShipmentQueryHandler> _logger;
    public GetListShipmentQueryHandler(ShipmentDbContext dbContext, ICustomerService customerService, ICustomerBatchLookupService customerBatchLookupService, ILogger<GetListShipmentQueryHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
        _customerService = customerService;
        _customerBatchLookupService = customerBatchLookupService;
    }

    public async Task<PageResponse<GetListShipmentQueryResponse>> Handle(GetListShipmentQuery query, CancellationToken cancellationToken)
    {
        try
        {
            var queryable = _dbContext.Shipments
                                .AsNoTracking()
                                .Include(c => c.Addresses)
                                .Include(c => c.ShipmentPackages)
                                .ThenInclude(sp => sp.Package)
                                .ThenInclude(addr => addr.PackageAdresses)
                                .OrderBy(c => c.CreateAt);

            var totalElements = await queryable.CountAsync(cancellationToken);

            var shipments = await queryable
                .Skip((query.Page - 1) * query.Size)
                .Take(query.Size)
                .ToListAsync(cancellationToken);

            var customerIds = shipments.Select(s => s.CustomerId).Distinct().ToList();
            var customers = await _customerBatchLookupService.GetListCustomerByIds(customerIds);

            return new PageResponse<GetListShipmentQueryResponse>
            {
                Status = true,
                Message = "Get list shipment success",
                Data = new Data<GetListShipmentQueryResponse>
                {
                    Content = shipments.Select(s => new GetListShipmentQueryResponse
                    {
                        Id = s.Id,
                        Customer = customers.ContainsKey(s.CustomerId) ? new Customer
                        {
                            Id = s.CustomerId,
                            Name = customers[s.CustomerId].FullName
                        } : null!,
                        Addresses = s.Addresses.Select(a => new ShipmentAddressSMDto
                        {
                            Id = a.Id,
                            District = a.District,
                            Name = a.Name,
                            Phone = a.Phone,
                            PhoneNumber = a.PhoneNumber,
                            PostCode = a.PostCode,
                            PrefixPhone = a.PrefixPhone,
                            ShipmentId = a.ShipmentId,
                            Ward = a.Ward,
                            Type = a.Type,
                            Address = a.Address,
                            City = a.City,
                            Code = a.Code,
                            CreateAt = a.CreateAt
                        }).ToList(),
                        Packages = s.ShipmentPackages.Select(sp => new PackageSMDto
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
                            PackageAddresses = sp.Package.PackageAdresses.Select(p => new PackageAddressCreateSMDTO
                            {
                                Id = Guid.NewGuid(),
                                District = p.District,
                                Longitude = p.Longitude,
                                Latitude = p.Latitude,
                                EstimatedDeliveryDate = p.EstimatedDeliveryDate,
                                DeliveryInstructions = p.DeliveryInstructions,
                                PackageId = sp.Package.Id,
                                Name = p.Name,
                                Phone = p.Phone,
                                PrefixPhone = p.PrefixPhone,
                                PostCode = p.PostCode,
                                Ward = p.Ward,
                                Status = p.Status,
                                Address = p.Address,
                                City = p.City,
                                Code = p.Code,
                                Country = p.Country,
                                UpdatedAt = DateTime.UtcNow,
                                UpdatedBy = p.UpdateBy
                            }).ToList()
                        }).ToList(),
                        ShipmentNumber = s.ShipmentNumber,
                        Note = s.Note,
                        Status = s.Status,
                        TotalAmount = s.TotalAmount,
                        Weight = s.Weight,
                        WarehouseId = s.WarehouseId
                    }).ToList(),
                    Page = query.Page,
                    Size = query.Size,
                    TotalElements = totalElements,
                    TotalPages = (int)Math.Ceiling(totalElements / (double)query.Size),
                    NumberOfElements = shipments.Count
                }
            };

        }
        catch (Exception Ex)
        {
            _logger.LogError(Ex, "Error fetching shipment list");
            return new PageResponse<GetListShipmentQueryResponse>
            {
                Status = false,
                Message = "An error occurred while fetching customer list.",
                Data = new Data<GetListShipmentQueryResponse>
                {
                    Content = new List<GetListShipmentQueryResponse>(),
                    Page = query.Page,
                    Size = query.Size,
                    TotalElements = 0,
                    TotalPages = 0,
                    NumberOfElements = 0
                }
            };
        }
    }
}
