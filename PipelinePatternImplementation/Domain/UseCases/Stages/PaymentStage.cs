using Domain.Enums;
using Domain.Models;
using Domain.Ports.InBound;
using Domain.ValueObjects;
using PipelinePatternImplementation.Domain.ValueObjects;

namespace Domain.UseCases.Payment
{
    public class PaymentStage : BaseUseCase, IOrderProcessingStage
    {
        public BaseReturn<Order> Process(Order order)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(order);

                if (order.status != OrderStatus.Validated)
                {
                    order.setStatus(OrderStatus.Failed);

                    return ErroNegocio<Order>(
                        1002,
                        "O pedido precisa estar validado antes do pagamento.");
                }

                // Simulação de pagamento aprovado.
                order.setStatus(OrderStatus.Paid);

                return new BaseReturn<Order>().Sucesso(order);
            }
            catch (Exception ex)
            {
                return ErroSistema<Order>(ex, 5001);
            }
        }
    }
}