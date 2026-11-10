using Domain.Models;
using PipelinePatternImplementation.Domain.ValueObjects;

namespace Domain.Ports.InBound
{
    public interface IOrderProcessingPipeline
    {
        BaseReturn<Order> Execute(Order order);
    }
}