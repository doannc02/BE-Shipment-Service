using Ichiba.Shipment.Application.Shipments.Queries;

namespace Ichiba.Shipment.Application.Common.Mappings;

public static class PackageAddressMapping
{
    public static PackageAddressCreateSMDTO MapPackageAddress(PackageAddress? packageAddress)
    {
        if (packageAddress == null) return null!;
        return new PackageAddressCreateSMDTO
        {
            Id = packageAddress.Id,
            PackageId = packageAddress.PackageId,
            Type = packageAddress.Type,
            Address = packageAddress.Address,
            City = packageAddress.City,
            District = packageAddress.District,
            Ward = packageAddress.Ward,
            PostCode = packageAddress.PostCode,
            Phone = packageAddress.Phone,
            Name = packageAddress.Name
        };
    }
}
