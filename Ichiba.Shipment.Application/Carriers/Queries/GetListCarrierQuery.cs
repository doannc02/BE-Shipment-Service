using Ichiba.Shipment.Application.Common.BaseRequest;
using Ichiba.Shipment.Application.Common.BaseResponse;
using Ichiba.Shipment.Domain.Entities;
using Ichiba.Shipment.Infrastructure.Data;
using StackExchange.Redis;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;



namespace Ichiba.Shipment.Application.Carriers.Queries;

public class GetListCarrierQueryResponse
{
    public Guid Id { get; set; }
    public string Code { get; set; }
    public bool? lastmile_tracking { get; set; }
    public string? logo { get; set; }
    public ShippingMethod ShippingMethod { get; set; }
    public CarrierType Type { get; set; }
}

public class GetListCarrierQuery : QueryPage, IRequest<PageResponse<GetListCarrierQueryResponse>>
{
}

public class GetListCarrierQueryHandler : IRequestHandler<GetListCarrierQuery, PageResponse<GetListCarrierQueryResponse>>
{
    private readonly ShipmentDbContext _dbContext;
    private readonly ILogger<GetListCarrierQueryHandler> _logger;
    private readonly IDatabase _cache;
    public GetListCarrierQueryHandler(ShipmentDbContext dbContext, ILogger<GetListCarrierQueryHandler> logger, IConnectionMultiplexer _redis)
    {
        _dbContext = dbContext;
        _logger = logger;
        _cache = _redis.GetDatabase(0); 
    }
    public async Task<PageResponse<GetListCarrierQueryResponse>> Handle(GetListCarrierQuery query, CancellationToken cancellationToken)
    {
        try
        {
            string cacheKey = "carriers_list";
            var cachedData = await _cache.StringGetAsync(cacheKey);
            if (!cachedData.IsNullOrEmpty)
            {
                var cachedResult = JsonConvert.DeserializeObject<PageResponse<GetListCarrierQueryResponse>>(cachedData);
                return cachedResult;
            }

            var queryable = _dbContext.Carriers
                                .AsNoTracking()
                                .OrderBy(c => c.CreatedDate);

            var totalElements = await queryable.CountAsync(cancellationToken);

            var shipments = await queryable
                .Skip((query.Page - 1) * query.Size)
                .Take(query.Size)
                .ToListAsync(cancellationToken);

            var result = new PageResponse<GetListCarrierQueryResponse>
            {
                Status = true,
                Message = "Get list shipment success",
                Data = new Data<GetListCarrierQueryResponse>
                {
                    Content = shipments.Select(s => new GetListCarrierQueryResponse
                    {
                        Id = s.Id,
                        Type = s.Type,
                        Code = s.Code,
                        lastmile_tracking = s.lastmile_tracking,
                        logo = s.logo,
                        ShippingMethod = s.ShippingMethod
                    }).ToList(),
                    Page = query.Page,
                    Size = query.Size,
                    TotalElements = totalElements,
                    TotalPages = (int)Math.Ceiling(totalElements / (double)query.Size),
                    NumberOfElements = shipments.Count
                }
            };
            await _cache.StringSetAsync(cacheKey, JsonConvert.SerializeObject(result), TimeSpan.FromDays(365));
            return result;
        }
        catch (Exception Ex)
        {
            _logger.LogError(Ex, "Error fetching shipment list");
            return new PageResponse<GetListCarrierQueryResponse>
            {
                Status = false,
                Message = "An error occurred while fetching customer list.",
                Data = new Data<GetListCarrierQueryResponse>
                {
                    Content = new List<GetListCarrierQueryResponse>(),
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

