namespace Ichiba.Shipment.Application.Common.BaseRequest;

public class QueryPage
{
    public int Page { get; set; } = 1;
    public int Size { get; set; } = 20;
    public string Sort { get; set; } = "asc";
}
