using Domain.Models.Xml;

namespace Domain.Ports.OutBound
{
    public interface IXmlRepository
    {
        Task<List<Xml>> ReadTop10(
            CancellationToken cancellationToken = default);
    }
}