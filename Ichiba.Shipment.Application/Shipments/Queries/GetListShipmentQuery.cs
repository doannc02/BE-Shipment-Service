using Ichiba.Shipment.Application.Common.BaseRequest;
using Ichiba.Shipment.Application.Common.BaseResponse;
using Ichiba.Shipment.Application.Common.Mappings;
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

public class CarrierSMView
{
    public Guid Id { get; set; }
    public string Code { get; set; }
    public bool? lastmile_tracking { get; set; }
    public string? logo { get; set; }
    public ShippingMethod ShippingMethod { get; set; }
    public CarrierType Type { get; set; }
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
public class GetListShipmentQuery : QueryPage, IRequest<PageResponse<GetShipmentDetailQueryResponse>>
{
}

public class GetListShipmentQueryHandler : IRequestHandler<GetListShipmentQuery, PageResponse<GetShipmentDetailQueryResponse>>
{
    private readonly ShipmentDbContext _dbContext;
    private readonly ICustomerBatchLookupService _customerBatchLookupService;
    private readonly ILogger<GetListShipmentQueryHandler> _logger;
    public GetListShipmentQueryHandler(ShipmentDbContext dbContext, ICustomerService customerService, ICustomerBatchLookupService customerBatchLookupService, ILogger<GetListShipmentQueryHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
        _customerBatchLookupService = customerBatchLookupService;
    }

    public async Task<PageResponse<GetShipmentDetailQueryResponse>> Handle(GetListShipmentQuery query, CancellationToken cancellationToken)
    {
        try
        {
            // Truy vấn tất cả shipment
            var queryable = _dbContext.Shipments
                .AsNoTracking()
                .Include(c => c.Addresses)
                .Include(c => c.ShipmentPackages)
                    .ThenInclude(sp => sp.Package)
                    .ThenInclude(addr => addr.PackageAdresses)
                .OrderBy(c => c.CreateAt);

            var totalElements = await queryable.CountAsync(cancellationToken);
            if (totalElements > 0)
            {
                var warehouseIds = await queryable.Select(s => s.WarehouseId).Distinct().ToListAsync(cancellationToken);
                var warehouses = await _dbContext.Warehouses
                    .Where(c => warehouseIds.Contains(c.Id))
                    .AsNoTracking()
                    .ToListAsync(cancellationToken);
                var warehouseDictionary = warehouses.ToDictionary(c => c.Id);

                // Lấy danh sách tất cả các CarrierId từ các Shipment
                var carrierIds = await queryable.Select(s => s.CarrierId).Distinct().ToListAsync(cancellationToken);

                // Lấy thông tin Carrier cho tất cả các CarrierId đã tìm thấy
                var carriers = await _dbContext.Carriers
                    .Where(c => carrierIds.Contains(c.Id))
                    .AsNoTracking()
                    .ToListAsync(cancellationToken);

                // hashmap id - carrier
                var carrierDictionary = carriers.ToDictionary(c => c.Id);

                // Lấy danh sách Shipment
                var shipments = await queryable
                    .Skip((query.Page - 1) * query.Size)
                    .Take(query.Size)
                    .ToListAsync(cancellationToken);

                // Lấy các CustomerId để tra cứu khách hàng
                var customerIds = shipments.Select(s => s.CustomerId).Distinct().ToList();
                var customers = await _customerBatchLookupService.GetListCustomerByIds(customerIds);

                return new PageResponse<GetShipmentDetailQueryResponse>
                {
                    Status = true,
                    Message = "Get list shipment success",
                    Data = new Data<GetShipmentDetailQueryResponse>
                    {
                        Content = shipments.Select(s => new GetShipmentDetailQueryResponse
                        {

                            Id = s.Id,
                            Customer = customers.ContainsKey(s.CustomerId) ? new Customer
                            {
                                Id = s.CustomerId,
                                Name = customers[s.CustomerId].FullName
                            } : null!,
                            AddressReceive = ShipmentAddressMapping.MapShipmentAddress(s.Addresses.FirstOrDefault(a => a.Type == ShipmentAddressType.ReceiveAddress)),
                            AddressSender = ShipmentAddressMapping.MapShipmentAddress(s.Addresses.FirstOrDefault(a => a.Type == ShipmentAddressType.SenderAddress)),
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
                                AddressSender = PackageAddressMapping.MapPackageAddress(sp.Package.PackageAdresses.FirstOrDefault(a => a.Type == ShipmentAddressType.SenderAddress)),
                                AddressReceive = PackageAddressMapping.MapPackageAddress(sp.Package.PackageAdresses.FirstOrDefault(a => a.Type == ShipmentAddressType.ReceiveAddress)),
                            }).ToList(),
                            ShipmentNumber = s.ShipmentNumber,
                            Note = s.Note,
                            Status = s.Status,
                            TotalAmount = s.TotalAmount,
                            Weight = s.Weight,
                            Carrier = carrierDictionary.ContainsKey(s.CarrierId) ? new CarrierSMView
                            {
                                Id = carrierDictionary[s.CarrierId].Id,
                                Code = carrierDictionary[s.CarrierId].Code,
                                lastmile_tracking = carrierDictionary[s.CarrierId].lastmile_tracking,
                                logo = carrierDictionary[s.CarrierId].logo,
                                ShippingMethod = carrierDictionary[s.CarrierId].ShippingMethod,
                                Type = carrierDictionary[s.CarrierId].Type
                            } : null!,
                            Warehouse = warehouseDictionary.ContainsKey(s.WarehouseId) ? new
                            {
                                id = warehouseDictionary[s.WarehouseId].Id,
                                name = warehouseDictionary[s.WarehouseId].Name,
                                ward = warehouseDictionary[s.WarehouseId].Ward,
                                country = warehouseDictionary[s.WarehouseId].Country,
                                address = warehouseDictionary[s.WarehouseId].Address,
                                logo = warehouseDictionary[s.WarehouseId].Logo
                            } : null!
                        }).ToList(),
                        Page = query.Page,
                        Size = query.Size,
                        TotalElements = totalElements,
                        TotalPages = (int)Math.Ceiling(totalElements / (double)query.Size),
                        NumberOfElements = shipments.Count
                    }
                };
            }
            else
            {
                return new PageResponse<GetShipmentDetailQueryResponse>
                {
                    Status = false,
                    Message = "No shipments found.",
                    Data = new Data<GetShipmentDetailQueryResponse>
                    {
                        Content = new List<GetShipmentDetailQueryResponse>(),
                        Page = query.Page,
                        Size = query.Size,
                        TotalElements = 0,
                        TotalPages = 0,
                        NumberOfElements = 0
                    }
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching shipment list");
            return new PageResponse<GetShipmentDetailQueryResponse>
            {
                Status = false,
                Message = "An error occurred while fetching customer list.",
                Data = new Data<GetShipmentDetailQueryResponse>
                {
                    Content = new List<GetShipmentDetailQueryResponse>(),
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
