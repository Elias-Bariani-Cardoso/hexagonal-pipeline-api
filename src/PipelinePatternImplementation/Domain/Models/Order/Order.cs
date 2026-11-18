using Domain.Enums;

namespace Domain.Models.Order
{
    public class Order
    {
        public string orderId { get; set; } = string.Empty;

        public decimal amount { get; set; }

        public OrderStatus status { get; private set; }
            = OrderStatus.Created;

        public decimal getAmount()
        {
            return amount;
        }

        public OrderStatus getStatus()
        {
            return status;
        }

        public void setStatus(OrderStatus value)
        {
            status = value;
        }
    }
}