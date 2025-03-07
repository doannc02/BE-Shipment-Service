using Ichiba.Shipment.Application.Common.BaseRequest;
using Ichiba.Shipment.Application.Common.BaseResponse;
using Ichiba.Shipment.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ichiba.Shipment.Application.Warehouses.Queries
{
    public class GetListWarehouseQuery
    {
        public class GetListWarehouseResponse
        {
            public Guid Id { get; set; }
            public string Logo { get; set; }
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
            public DateTime CreateAt { get; set; } = DateTime.UtcNow;
            public Guid? CreateBy { get; set; }
            public string? Country { get; set; }
            public double? Latitude { get; set; }
            public double? Longitude { get; set; }
        }

        public class QueryGetListWarehouse : QueryPage, IRequest<PageResponse<GetListWarehouseResponse>>
        {
            public string? Search { get; set; } 
            public string? SortBy { get; set; } = "CreateAt"; 
            public bool IsDescending { get; set; } = true; 
        }

        public class GetListWarehouseQueryHandler : IRequestHandler<QueryGetListWarehouse, PageResponse<GetListWarehouseResponse>>
        {
            private readonly ShipmentDbContext _dbContext;
            private readonly ILogger<GetListWarehouseQueryHandler> _logger;

            public GetListWarehouseQueryHandler(ShipmentDbContext dbContext, ILogger<GetListWarehouseQueryHandler> logger)
            {
                _dbContext = dbContext;
                _logger = logger;
            }

            public async Task<PageResponse<GetListWarehouseResponse>> Handle(QueryGetListWarehouse request, CancellationToken cancellationToken)
            {
                try
                {
                    var query = _dbContext.Warehouses.AsQueryable();

                    if (!string.IsNullOrWhiteSpace(request.Search))
                    {
                        query = query.Where(w =>
                            w.Name.Contains(request.Search) ||
                            w.Code.Contains(request.Search));
                    }

                    var totalRecords = await query.CountAsync(cancellationToken);

                    if (!string.IsNullOrEmpty(request.SortBy))
                    {
                        query = request.IsDescending
                            ? query.OrderByDescending(w => EF.Property<object>(w, request.SortBy))
                            : query.OrderBy(w => EF.Property<object>(w, request.SortBy));
                    }

                    var warehouses = await query
                        .Skip((request.Page - 1) * request.Size)
                        .Take(request.Size)
                        .Select(w => new GetListWarehouseResponse
                        {
                            Id = w.Id,
                            Logo = w.Logo,
                            Name = w.Name,
                            PrefixPhone = w.PrefixPhone,
                            PhoneNumber = w.PhoneNumber,
                            Code = w.Code,
                            Phone = w.Phone,
                            City = w.City,
                            District = w.District,
                            Ward = w.Ward,
                            PostCode = w.PostCode,
                            Address = w.Address,
                            CreateAt = w.CreateAt,
                            CreateBy = w.CreateBy,
                            Country = w.Country,
                            Latitude = w.Latitude,
                            Longitude = w.Longitude
                        })
                        .ToListAsync(cancellationToken);

                    return new PageResponse<GetListWarehouseResponse>
                    {
                        Data = new Data<GetListWarehouseResponse>
                        {
                            Content = warehouses,
                            TotalElements = totalRecords,
                            Page = request.Page,
                            Size = request.Size
                        },
                        Status = true,
                        Message = "Get list warehouse success!"
                    };
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error fetching warehouse list");
                    throw;
                }
            }
        }
    }
}
