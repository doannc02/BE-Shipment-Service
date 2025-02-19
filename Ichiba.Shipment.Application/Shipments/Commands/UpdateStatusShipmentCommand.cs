using Ichiba.Shipment.Application.Common.BaseResponse;
using Ichiba.Shipment.Domain.Entities;
using Ichiba.Shipment.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ichiba.Shipment.Application.Shipments.Commands
{
    public class UpdateStatusShipmentCommandResponse
    {
        public Guid Id { get; set; }
    }

    public class UpdateStatusShipmentCommand : IRequest<BaseEntity<UpdateStatusShipmentCommandResponse>>
    {
        public required Guid Id { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public required ShipmentStatus Status { get; set; } = ShipmentStatus.ShipmentCreated;
        public required Guid UpdatedBy { get; set; }
        public required DateTime UpdatedAt { get; set; }
    }

    public class UpdateStatusShipmentCommandHandler : IRequestHandler<UpdateStatusShipmentCommand, BaseEntity<UpdateStatusShipmentCommandResponse>>
    {
        private readonly ShipmentDbContext _dbContext;
        private readonly ILogger<UpdateStatusShipmentCommandHandler> _logger;

        public UpdateStatusShipmentCommandHandler(ShipmentDbContext dbContext, ILogger<UpdateStatusShipmentCommandHandler> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<BaseEntity<UpdateStatusShipmentCommandResponse>> Handle(UpdateStatusShipmentCommand request, CancellationToken cancellationToken)
        {
            var shipment = await _dbContext.Shipments.SingleOrDefaultAsync(i => i.Id == request.Id, cancellationToken);

            if (shipment == null)
            {
                _logger.LogWarning($"Shipment with ID {request.Id} not found.");
                return new BaseEntity<UpdateStatusShipmentCommandResponse>
                {
                    Status = false,
                    Message = "Shipment not found."
                };
            }

            shipment.Status = request.Status;
            shipment.UpdateBy = request.UpdatedBy;
            shipment.UpdateAt = request.UpdatedAt;

            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation($"Shipment {request.Id} status updated to {request.Status} by {request.UpdatedBy} at {request.UpdatedAt}");

            return new BaseEntity<UpdateStatusShipmentCommandResponse>
            {
                Status = true,
                Message = "Cập nhật trạng thái thành công.",
                Data = new UpdateStatusShipmentCommandResponse { Id = shipment.Id }
            };
        }
    }
}
