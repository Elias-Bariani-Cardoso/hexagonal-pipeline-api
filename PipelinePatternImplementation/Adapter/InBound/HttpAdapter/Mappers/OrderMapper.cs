using Adapter.InBound.HttpAdapter.DTOs.Order;
using Domain.Enums;
using Domain.Models;

namespace Adapter.InBound.HttpAdapter.Mappers
{
    public static class OrderMapper
    {
        public static Order OrderRequestToDomainMapper(
            OrderRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            if (string.IsNullOrWhiteSpace(request.orderId))
            {
                throw new ArgumentException(
                    "O campo orderId é obrigatório.",
                    nameof(request));
            }

            var order = new Order
            {
                orderId = request.orderId,

                amount = request.amount
                    ?? throw new ArgumentException(
                        "O campo amount é obrigatório.",
                        nameof(request))
            };

            order.setStatus(
                OrderStatusConverter.FromText(request.status));

            return order;
        }

        public static OrderResponse OrderDomainToResponseMapper(
            Order order)
        {
            ArgumentNullException.ThrowIfNull(order);

            return new OrderResponse
            {
                orderId = order.orderId,
                amount = order.amount,
                status = order.status.ToText()
            };
        }
    }
}