using Ichiba.Shipment.Application.Shipments.Queries;
using Ichiba.Shipment.Domain.Entities;

namespace Ichiba.Shipment.Application.Common.Mappings;

public static class ShipmentAddressMapping
{
    public static ShipmentAddressSMDto MapShipmentAddress(ShipmentAddress? address)
    {
        if (address == null) return null!;
        return new ShipmentAddressSMDto
        {
            Id = address.Id,
            ShipmentId = address.ShipmentId,
            Type = address.Type,
            Address = address.Address,
            City = address.City,
            District = address.District,
            Ward = address.Ward,
            PostCode = address.PostCode,
            Phone = address.Phone,
            Name = address.Name
        };
    }
}
