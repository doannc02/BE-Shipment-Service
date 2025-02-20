using Ichiba.Shipment.Application.Common.BaseRequest;
using Ichiba.Shipment.Application.Common.BaseResponse;
using Ichiba.Shipment.Domain.Entities;
using Ichiba.Shipment.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Ichiba.Shipment.Application.Carriers.Queries;

public class GetDetailCarrierQueryResponse
{
    public  Guid Id { get; set; }
    public  string Code { get; set; }
    public bool? lastmile_tracking { get; set; }
    public string? logo { get; set; }
    public ShippingMethod ShippingMethod { get; set; }
    public CarrierType Type { get; set; }
}

public class GetDetailCarrierQuery : QueryDetail,IRequest<BaseEntity<GetDetailCarrierQueryResponse>>
{
}

public class GetDetailCarrierQueryHandler : IRequestHandler<GetDetailCarrierQuery, BaseEntity<GetDetailCarrierQueryResponse>>
{
    private readonly ShipmentDbContext _dbContext;
    private readonly ILogger<GetDetailCarrierQueryHandler> _logger;
    public GetDetailCarrierQueryHandler(ShipmentDbContext dbContext, ILogger<GetDetailCarrierQueryHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }
    public async Task<BaseEntity<GetDetailCarrierQueryResponse>> Handle(GetDetailCarrierQuery query, CancellationToken cancellation)
    {
        var existCarrirer = await _dbContext.Carriers.SingleOrDefaultAsync(i => i.Id == query.Id);
        if (existCarrirer == null) return new BaseEntity<GetDetailCarrierQueryResponse>
        {
            Status = false,
            Message = $"Not found carrier by id {query.Id}"
        };

        return new BaseEntity<GetDetailCarrierQueryResponse>
        {
            Status = true,
            Data = new GetDetailCarrierQueryResponse
            {
                Id = existCarrirer.Id,
                Code = existCarrirer.Code,
                lastmile_tracking = existCarrirer.lastmile_tracking,
                logo = existCarrirer.logo,
                ShippingMethod = existCarrirer.ShippingMethod,
                Type = existCarrirer.Type
            }
        };
    }
} 

