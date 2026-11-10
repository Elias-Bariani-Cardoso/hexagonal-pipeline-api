using Domain.Models;
using PipelinePatternImplementation.Domain.ValueObjects;

namespace Domain.Ports.InBound
{
    public interface IOrderProcessingStage
    {
        BaseReturn<Order> Process(Order order);
    }
}
