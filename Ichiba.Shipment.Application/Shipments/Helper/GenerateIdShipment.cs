namespace Ichiba.Shipment.Application.Shipments.Helper;

public static class  GenerateIdShipment
{
    public static async Task<string> GenShipmentNumber(Guid guid)
    {
        string prefix = "SJA";
        string datePart = DateTime.UtcNow.ToString("yyMM");
        string valueSignature = guid.ToString("N").Substring(0, 4);
        return $"{prefix}{datePart}{valueSignature}";
    }
}
