using Domain.Models.Xml;
using PipelinePatternImplementation.Domain.ValueObjects;

namespace Domain.Ports.InBound
{
    public interface IXmlUseCase
    {
        Task<BaseReturn<List<Xml>>> ExecutarAsync(
            CancellationToken cancellationToken = default);
    }
}