using Ichiba.Shipment.Application.Common.BaseRequest;
using Ichiba.Shipment.Application.Common.BaseResponse;
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
    public string Status { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public decimal Weight { get; set; }
    public DateTime CreateAt { get; set; } = DateTime.UtcNow;
    public Guid? CreateBy { get; set; }
    public DateTime? UpdateAt { get; set; }
    public Guid? UpdateBy { get; set; }
    public DateTime? DeleteAt { get; set; }
    public Guid? DeleteBy { get; set; }

    //public virtual List<ShipmentAddress> Addresses { get; set; } = new();
    //public virtual List<ShipmentPackage> ShipmentPackages { get; set; } = new();
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
    public GetListShipmentQueryHandler(ShipmentDbContext dbContext, ICustomerService customerService, ICustomerBatchLookupService customerBatchLookupService,  ILogger<GetListShipmentQueryHandler> logger)
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
            var queryable = _dbContext.Shipments.AsNoTracking().OrderBy(c => c.CreateAt);

            var totalElements = await queryable.CountAsync(cancellationToken);
            var shipments = await queryable.Skip((query.Page - 1) * query.Size).Take(query.Size).ToListAsync(cancellationToken);
            var customerIds = shipments.Select(s => s.CustomerId).Distinct().ToList();
            var customers = await _customerBatchLookupService.GetListCustomerByIds(customerIds);
            return new PageResponse<GetListShipmentQueryResponse>
            {
                Status = true,
                Message = "Get list customer Success",
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
