namespace Ichiba.Shipment.Domain.Entities;

public class PackageProduct
{
    public required Guid Id { get; set; }
    public Guid PackageId { get; set; }
    public virtual Package Package { get; set; }
    public string ProductName { get; set; }
    public Guid ProductId { get; set; }
    public string Origin { get; set; }
    public double OriginPrice { get; set; }
    public int Quantity { get; set; }
    public double Total { get; set; }
    public double Unit { get; set; }
    public string ProductLink { get; set; }
    public List<Tax> Taxes { get; set; }
}


public class Tax
{
    public Guid Id { get; set; }
    public string Name { get; set; }
}