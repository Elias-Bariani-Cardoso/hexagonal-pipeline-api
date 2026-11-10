using Domain.Enums;
using Domain.Models;
using Domain.Ports.InBound;
using Domain.ValueObjects;
using PipelinePatternImplementation.Domain.ValueObjects;

namespace Domain.Services
{
    public class OrderProcessingPipeline
        : BaseUseCase, IOrderProcessingPipeline
    {
        private readonly IReadOnlyList<IOrderProcessingStage> _stages;

        public OrderProcessingPipeline(IServiceProvider serviceProvider)
        {
            ArgumentNullException.ThrowIfNull(serviceProvider);

            _stages = serviceProvider
                .GetRequiredService<IEnumerable<IOrderProcessingStage>>()
                .ToArray();
        }

        public BaseReturn<Order> Execute(Order order)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(order);

                foreach (var stage in _stages)
                {
                    var resultado = stage.Process(order);

                    if (resultado.Status != EnumStatus.SUCESSO)
                    {
                        return resultado;
                    }

                    order = resultado.SuccessObject
                        ?? throw new InvalidOperationException(
                            $"O stage {stage.GetType().Name} " +
                            "retornou sucesso sem um pedido.");
                }

                return new BaseReturn<Order>().Sucesso(order);
            }
            catch (Exception ex)
            {
                return ErroSistema<Order>(ex, 5000);
            }
        }
    }
}