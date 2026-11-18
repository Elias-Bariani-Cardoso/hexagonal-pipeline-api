namespace Adapter.InBound.HttpAdapter.DTOs.Order
{
    public class OrderResponse
    {
        public string orderId { get; set; } = string.Empty;
        public decimal amount { get; set; }
        public string status { get; set; } = string.Empty;
    }
}