using Domain.Enums;
using Domain.Models;
using Domain.Ports.InBound;
using Domain.ValueObjects;
using PipelinePatternImplementation.Domain.ValueObjects;

namespace Domain.UseCases.Validation
{
    public class ValidationStage : BaseUseCase, IOrderProcessingStage
    {
        public BaseReturn<Order> Process(Order order)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(order);

                if (order.amount <= 0)
                {
                    order.setStatus(OrderStatus.Invalid);

                    return ErroNegocio<Order>(
                        1001,
                        "O valor do pedido deve ser maior que zero.");
                }

                order.setStatus(OrderStatus.Validated);

                return new BaseReturn<Order>().Sucesso(order);
            }
            catch (Exception ex)
            {
                return ErroSistema<Order>(ex, 5000);
            }
        }
    }
}