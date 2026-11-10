using Domain.Enums;
using Domain.Models;
using Domain.Ports.InBound;
using Domain.ValueObjects;
using PipelinePatternImplementation.Domain.ValueObjects;

namespace Domain.UseCases.Shipping
{
    public class ShippingStage : BaseUseCase, IOrderProcessingStage
    {
        public BaseReturn<Order> Process(Order order)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(order);

                if (order.status != OrderStatus.Paid)
                {
                    order.setStatus(OrderStatus.Failed);

                    return ErroNegocio<Order>(
                        1003,
                        "O pedido precisa estar pago antes do envio.");
                }

                // Simulação de envio realizado.
                order.setStatus(OrderStatus.Shipped);

                return new BaseReturn<Order>().Sucesso(order);
            }
            catch (Exception ex)
            {
                return ErroSistema<Order>(ex, 5002);
            }
        }
    }
}