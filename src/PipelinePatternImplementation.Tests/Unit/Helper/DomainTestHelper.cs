using Domain.Models.Order;
using Domain.ValueObjects;
using PipelinePatternImplementation.Domain.ValueObjects;

namespace Unit.Helper
{
    internal sealed class DomainTestHelper : BaseUseCase
    {
        public static Order CreateOrder(decimal amount = 100m)
        {
            return new Order
            {
                amount = amount
            };
        }

        public static BaseReturn<Order> Success(Order order)
        {
            return new BaseReturn<Order>().Sucesso(order);
        }

        public static BaseReturn<Order> BusinessError()
        {
            return ErroNegocio<Order>(
                1001,
                "Erro de negócio simulado.");
        }

        public static BaseReturn<Order> SystemError()
        {
            return ErroSistema<Order>(
                new InvalidOperationException(
                    "Erro de sistema simulado."),
                5000);
        }
    }
}
