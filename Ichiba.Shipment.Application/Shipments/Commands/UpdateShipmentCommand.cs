using AutoMapper;
using Ichiba.Shipment.Application.Common.BaseResponse;
using Ichiba.Shipment.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Ichiba.Shipment.Application.Shipments.Commands;

public class UpdateShipmentResponse
{
    public Guid Id { get; set; }
}

public class UpdateShipmentCommand : IRequest<BaseEntity<UpdateShipmentResponse>>
{
    public required Guid Id { get; set; }
    public string? Note { get; set; }
    public required Guid UpdatedBy { get; set; }
    public decimal? TotalAmount { get; set; }
    public decimal? Weight { get; set; }
    public decimal? Height { get; set; }
}

public class UpdateShipmentCommandHandler : IRequestHandler<UpdateShipmentCommand, BaseEntity<UpdateShipmentResponse>>
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;
    private readonly IShipmentRepository _shipmentRepository;
    private readonly ILogger<UpdateShipmentCommandHandler> _logger;

    public UpdateShipmentCommandHandler(IMapper mapper, IMediator mediator, ILogger<UpdateShipmentCommandHandler> logger, IShipmentRepository shipmentRepository)
    {
        _logger = logger;
        _mapper = mapper;
        _shipmentRepository = shipmentRepository;
        _mediator = mediator;
    }

    public async Task<BaseEntity<UpdateShipmentResponse>> Handle(UpdateShipmentCommand request, CancellationToken cancellationToken)
    {
        if (request.Id == Guid.Empty)
        {
            return new BaseEntity<UpdateShipmentResponse>
            {
                Status = false,
                Message = "Shipment ID không hợp lệ."
            };
        }

        var entity = await _shipmentRepository.GetAsync(request.Id);
        if (entity == null)
        {
            return new BaseEntity<UpdateShipmentResponse>
            {
                Status = false,
                Message = "Không tìm thấy shipment."
            };
        }

        if (request.TotalAmount < 0)
        {
            return new BaseEntity<UpdateShipmentResponse>
            {
                Status = false,
                Message = "TotalAmount không được nhỏ hơn 0."
            };
        }

        if (request.Weight < 0)
        {
            return new BaseEntity<UpdateShipmentResponse>
            {
                Status = false,
                Message = "Weight không được nhỏ hơn 0."
            };
        }

        if (request.Height < 0)
        {
            return new BaseEntity<UpdateShipmentResponse>
            {
                Status = false,
                Message = "Height không được nhỏ hơn 0."
            };
        }

        entity.Weight = request.Weight ?? entity.Weight;
        entity.Height = request.Height ?? entity.Height;
        entity.UpdateAt = DateTime.UtcNow;
        entity.UpdateBy = request.UpdatedBy;
        entity.Note = request.Note ?? entity.Note;
        entity.TotalAmount = request.TotalAmount ?? entity.TotalAmount;

        await _shipmentRepository.UpdateAsync(entity);

        _logger.LogInformation($"Shipment {request.Id} updated successfully.");

        return new BaseEntity<UpdateShipmentResponse>
        {
            Data = new UpdateShipmentResponse { Id = entity.Id },
            Message = "Cập nhật thành công.",
            Status = true
        };
    }
}
