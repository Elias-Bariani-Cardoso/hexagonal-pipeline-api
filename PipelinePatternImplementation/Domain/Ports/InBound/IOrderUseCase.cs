using Domain.Models;
using PipelinePatternImplementation.Domain.ValueObjects;

namespace Domain.Ports.InBound
{
    public interface IOrderUseCase
    {
        Task<BaseReturn<Order>> ExecutarAsync(Order order);
    }
}