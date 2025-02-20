using Ichiba.Shipment.Application.Common.BaseRequest;
using Ichiba.Shipment.Application.Common.BaseResponse;
using Ichiba.Shipment.Domain.Consts;
using Ichiba.Shipment.Domain.Entities;
using Ichiba.Shipment.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Ichiba.Shipment.Application.ShipmentPackages.Queries;

public class GetDetailShipmentPackageQueryResponse
{
    public Guid Id { get; set; }
    public Guid ShipmentId { get; set; }
    public Guid PackageId { get; set; }
    public virtual PackageView Package { get; set; }
    public virtual ShipmentDTO Shipment { get; set; }
    public DateTime CreateAt { get; set; }
    public Guid? CreateBy { get; set; }
    public DateTime? UpdateAt { get; set; }
    public Guid? UpdateBy { get; set; }
    public DateTime? DeleteAt { get; set; }
    public Guid? DeleteBy { get; set; }
}

public class PackageView
{
    public Guid CustomerId { get; set; }
   // public virtual ShipmentAddress ShipmentAddress { get; set; }
    //public required Guid ShipmentAddressId { get; set; }
    public Guid Id { get; set; }
    public Guid WarehouseId { get; set; }
    public string PackageNumber { get; set; }
    public string? Note { get; set; }
    public PackageStatus Status { get; set; }
    public decimal Length { get; set; }
    public decimal Width { get; set; }
    public decimal Height { get; set; }
    public decimal Weight { get; set; }
    public DateTime CreateAt { get; set; }
    public Guid? CreateBy { get; set; }
    public DateTime? UpdateAt { get; set; }
    public Guid? UpdateBy { get; set; }
    public DateTime? DeleteAt { get; set; }
    public Guid? DeleteBy { get; set; }
}

public class ShipmentAdressView
{
    public Guid Id { get; set; }
    public Guid ShipmentId { get; set; }
    public string Name { get; set; }
    public string? PrefixPhone { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Code { get; set; }
    public string? Phone { get; set; }
    public string? City { get; set; }
    public string? District { get; set; }
    public string? Ward { get; set; }
    public string? PostCode { get; set; }
    public string Address { get; set; }
    public ShipmentAddressType Type { get; set; }
    public DateTime CreateAt { get; set; }
    public Guid? CreateBy { get; set; }
    public DateTime? UpdateAt { get; set; }
    public Guid? UpdateBy { get; set; }
    public DateTime? DeleteAt { get; set; }
    public Guid? DeleteBy { get; set; }
}

public class ShipmentDTO
{
    public Guid Id { get; set; }
    public Guid WarehouseId { get; set; }
    public Guid CustomerId { get; set; }
    public string ShipmentNumber { get; set; }
    public string? Note { get; set; }
    public ShipmentStatus Status { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal Weight { get; set; }
    public decimal Height { get; set; }
}

public class GetDetailShipmentPackageQuery : QueryDetail, IRequest<BaseEntity<GetDetailShipmentPackageQueryResponse>>
{
}

public class GetDetailShipmentPackageQueryHandler : IRequestHandler<GetDetailShipmentPackageQuery, BaseEntity<GetDetailShipmentPackageQueryResponse>>
{
    private readonly ShipmentDbContext _dbContext;
    private readonly ILogger<GetDetailShipmentPackageQueryHandler> _logger;

    public GetDetailShipmentPackageQueryHandler(ShipmentDbContext dbContext, ILogger<GetDetailShipmentPackageQueryHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<BaseEntity<GetDetailShipmentPackageQueryResponse>> Handle(GetDetailShipmentPackageQuery request, CancellationToken cancellationToken)
    {
        var query = await _dbContext.ShipmentPackages
            .Include(x => x.Package)
            .Include(x => x.Shipment)
            .AsNoTracking()
            .SingleOrDefaultAsync(i => i.Id == request.Id, cancellationToken);

        if (query == null)
        {
            return new BaseEntity<GetDetailShipmentPackageQueryResponse>
            {
                Status = false,
                Message = $"Not found Shipment package by ID: {request.Id}"
            };
        }

        var response = new GetDetailShipmentPackageQueryResponse
        {
            Id = query.Id,
            ShipmentId = query.ShipmentId,
            PackageId = query.PackageId,
            Package = new PackageView
            {
                Id = query.Package.Id,
                CustomerId = query.Package.CustomerId,
              //  ShipmentAddressId = query.Package.ShipmentAddressId,
                WarehouseId = query.Package.WarehouseId,
                PackageNumber = query.Package.PackageNumber,
                Note = query.Package.Note,
                Status = query.Package.Status,
                Length = query.Package.Length,
                Width = query.Package.Width,
                Height = query.Package.Height,
                Weight = query.Package.Weight,
                CreateAt = query.Package.CreateAt,
                CreateBy = query.Package.CreateBy,
                UpdateAt = query.Package.UpdateAt,
                UpdateBy = query.Package.UpdateBy,
                DeleteAt = query.Package.DeleteAt,
                DeleteBy = query.Package.DeleteBy,
             //   ShipmentAddress = query.Package.ShipmentAddress
            },
            Shipment = new ShipmentDTO
            {
                Id = query.Shipment.Id,
                WarehouseId = query.Shipment.WarehouseId,
                CustomerId = query.Shipment.CustomerId,
                ShipmentNumber = query.Shipment.ShipmentNumber,
                Note = query.Shipment.Note,
                Status = query.Shipment.Status,
                TotalAmount = query.Shipment.TotalAmount,
                Weight = query.Shipment.Weight,
                Height = query.Shipment.Height
            },
            CreateAt = query.CreateAt,
            CreateBy = query.CreateBy,
            UpdateAt = query.UpdateAt,
            UpdateBy = query.UpdateBy,
            DeleteAt = query.DeleteAt,
            DeleteBy = query.DeleteBy
        };

        return new BaseEntity<GetDetailShipmentPackageQueryResponse>
        {
            Data = response,
            Status = true,
            Message = "Shipment package details retrieved successfully."
        };
    }
}