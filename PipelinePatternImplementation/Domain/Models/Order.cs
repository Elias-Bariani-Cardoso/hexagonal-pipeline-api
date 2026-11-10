using Domain.Enums;

namespace Domain.Models
{
    public class Order
    {
        public string orderId { get; set; } = string.Empty;

        public double amount { get; set; }

        public OrderStatus status { get; private set; }
            = OrderStatus.Created;

        public double getAmount()
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