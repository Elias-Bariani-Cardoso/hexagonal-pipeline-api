using Domain.Models.Order;
using PipelinePatternImplementation.Domain.ValueObjects;

namespace Domain.Ports.InBound
{
    public interface IOrderProcessingStage
    {
        BaseReturn<Order> Process(Order order);
    }
}
