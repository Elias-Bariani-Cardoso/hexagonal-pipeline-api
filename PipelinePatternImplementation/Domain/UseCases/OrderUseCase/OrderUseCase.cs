using Adapter.InBound.HttpAdapter.DTOs.Order;
using Domain.Enums;
using Domain.Ports.InBound;
using Domain.ValueObjects;
using PipelinePatternImplementation.Domain.ValueObjects;

namespace Domain.UseCases.OrderUseCase
{
    public class OrderUseCase : BaseUseCase, IOrderUseCase
    {
        private readonly IOrderProcessingPipeline _pipeline;

        public OrderUseCase(IOrderProcessingPipeline pipeline)
        {
            ArgumentNullException.ThrowIfNull(pipeline);

            _pipeline = pipeline;
        }

        public Task<BaseReturn<Models.Order>> ExecutarAsync(
            Models.Order order)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(order);

                return Task.FromResult(_pipeline.Execute(order));
            }
            catch (Exception ex)
            {
                return Task.FromResult(
                    ErroSistema<Models.Order>(ex, 5000));
            }
        }
    }
}
