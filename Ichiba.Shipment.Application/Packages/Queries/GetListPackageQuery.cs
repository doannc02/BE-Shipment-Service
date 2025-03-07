using Ichiba.Shipment.Application.Common.BaseRequest;
using Ichiba.Shipment.Application.Common.BaseResponse;
using Ichiba.Shipment.Application.Common.Mappings;
using Ichiba.Shipment.Application.Shipments.Queries;
using Ichiba.Shipment.Domain.Consts;
using Ichiba.Shipment.Infrastructure.Data;
using Ichiba.Shipment.Infrastructure.Services.Customers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Ichiba.Shipment.Application.Packages.Queries;



public class GetListPackageQuery : QueryPage, IRequest<PageResponse<GetDetailPackageQueryResponse>>
{
    public Guid? CustomerId { get; set; }
}

public class GetListPackageQueryHandler : IRequestHandler<GetListPackageQuery, PageResponse<GetDetailPackageQueryResponse>>
{
    private readonly ShipmentDbContext _dbContext;
    private readonly ILogger<GetListPackageQueryHandler> _logger;
    private readonly ICustomerBatchLookupService _customerBatchService;

    public GetListPackageQueryHandler(ShipmentDbContext dbContext, ILogger<GetListPackageQueryHandler> logger, ICustomerBatchLookupService customerBatchService)
    {
        _dbContext = dbContext;
        _logger = logger;
        _customerBatchService = customerBatchService;
    }

    public async Task<PageResponse<GetDetailPackageQueryResponse>> Handle(GetListPackageQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var packageQuery = _dbContext.Packages
                .AsNoTracking()
                .Include(p => p.PackageAdresses )
                .AsQueryable();
            var carrierIds = await packageQuery.Select(s => s.CarrierId).Distinct().ToListAsync(cancellationToken);
            
            var carriers = await _dbContext.Carriers
                .Where(c => carrierIds.Contains(c.Id))
                .AsNoTracking()
                .ToListAsync(cancellationToken);
            // hashmap id - carrier
            var carrierDictionary = carriers.ToDictionary(c => c.Id);
            if (request.CustomerId.HasValue && request.CustomerId != Guid.Empty)
            {
                packageQuery = packageQuery.Where(p => p.CustomerId == request.CustomerId);
            }

            if (!string.IsNullOrEmpty(request.Sort))
            {
                packageQuery = request.Sort.ToLower() switch
                {
                    "asc" => packageQuery.OrderBy(p => p.CreateAt),
                    "desc" => packageQuery.OrderByDescending(p => p.CreateAt),
                    _ => packageQuery
                };
            }

            var totalRecords = await packageQuery.CountAsync(cancellationToken);

            var packageList = await packageQuery
                .Skip((request.Page - 1) * request.Size)
                .Take(request.Size)
                .Select(p => new GetDetailPackageQueryResponse
                {
                    Id = p.Id,
                    AddressSender = PackageAddressMapping.MapPackageAddress(p.PackageAdresses.FirstOrDefault(i => i.Type == ShipmentAddressType.SenderAddress)),
                    AddressReceive = PackageAddressMapping.MapPackageAddress(p.PackageAdresses.FirstOrDefault(i => i.Type == ShipmentAddressType.ReceiveAddress)),
                    Note = p.Note,
                    Length = p.Length,
                    Width = p.Width,
                    Height = p.Height,
                    Weight = p.Weight,
                    UpdateBy = p.UpdateBy,
                    PackageProduct = p.PackageProducts.Any() ? p.PackageProducts.Select(pk => new PackageProductResponse
                    {
                        Id = pk.Id,
                        Origin = pk.Origin,
                        OriginPrice = pk.OriginPrice,
                        ProductLink = pk.ProductLink,
                        ProductName = pk.ProductName,
                        Quantity = pk.Quantity,
                        Tax = pk.Tax,
                        Total = pk.Total,
                        Unit = pk.Unit
                    }).ToList() : new List<PackageProductResponse>(),
                    Customer = new CustomerDTO()
                    {
                        Id = p.CustomerId,
                    },
                    Carrier = carrierDictionary.ContainsKey(p.CarrierId) ? new CarrierSMView
                    {
                        Id = carrierDictionary[p.CarrierId].Id,
                        Code = carrierDictionary[p.CarrierId].Code,
                        lastmile_tracking = carrierDictionary[p.CarrierId].lastmile_tracking,
                        logo = carrierDictionary[p.CarrierId].logo,
                        ShippingMethod = carrierDictionary[p.CarrierId].ShippingMethod,
                        Type = carrierDictionary[p.CarrierId].Type
                    } : null!
                })
                .ToListAsync(cancellationToken);

            var customerIds = packageList.Where(p => p.Customer != null).Select(p => p.Customer.Id).Distinct().ToList();
            var customers = await _customerBatchService.GetListCustomerByIds(customerIds);

            var responseList = packageList.Select(s =>
            {
                s.Customer = customers.TryGetValue(s.Customer.Id, out var customer) ? new CustomerDTO
                {
                    Id = customer.Id,
                    FullName = customer.FullName,
                } : new CustomerDTO();
                return s;
            }).ToList();

            return new PageResponse<GetDetailPackageQueryResponse>
            {
                Status = true,
                Message = "Get list package success",
                Data = new Data<GetDetailPackageQueryResponse>
                {
                    Content = responseList,
                    Page = request.Page,
                    Size = request.Size,
                    TotalElements = totalRecords,
                    TotalPages = (int)Math.Ceiling(totalRecords / (double)request.Size),
                    NumberOfElements = responseList.Count
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving package list");
            return new PageResponse<GetDetailPackageQueryResponse>
            {
                Status = false,
                Message = "An error occurred while retrieving the package list."
            };
        }
    }
}
