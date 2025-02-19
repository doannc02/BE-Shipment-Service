namespace Ichiba.Shipment.Application.Common.BaseResponse;
public class PageResponse<T>
{
    public bool Status { get; set; }
    public string Message { get; set; }
    public Data<T> Data { get; set; }
}
public class Data<T>
{
    public List<T> Content { get; set; }
    public int Page { get; set; }
    public int Size { get; set; }
    public string Sort { get; set; }
    public int TotalElements { get; set; }
    public int TotalPages { get; set; }
    public int NumberOfElements { get; set; }
}

