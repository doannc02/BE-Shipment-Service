using Microsoft.EntityFrameworkCore;
using Ichiba.Shipment.Infrastructure.Data;

namespace Ichiba.Shipment.Application.Products.Helper;

public static class PackageNumberGenerator
{
    public static async Task<string> GeneratePackageNumber(ShipmentDbContext dbContext, CancellationToken cancellationToken)
    {
        var currentDate = DateTime.UtcNow.ToString("yyMMdd");

        var maxPackageNumber = await dbContext.Packages
            .Where(p => p.PackageNumber.StartsWith("PK" + currentDate))
            .OrderByDescending(p => p.PackageNumber)
            .Select(p => p.PackageNumber)
            .FirstOrDefaultAsync(cancellationToken);

        int nextSequence = 1;
        if (!string.IsNullOrEmpty(maxPackageNumber))
        {
            int.TryParse(maxPackageNumber.Substring(8), out nextSequence);
            nextSequence++;
        }

        return $"PK{currentDate}{nextSequence:D4}";
    }
}
