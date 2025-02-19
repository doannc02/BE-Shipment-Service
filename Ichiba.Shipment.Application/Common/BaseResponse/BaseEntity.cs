namespace Ichiba.Shipment.Application.Common.BaseResponse;
public class BaseEntity<T>
{
    public bool Status { get; set; }
    public T Data { get; set; }
    public string Message { get; set; }
}
