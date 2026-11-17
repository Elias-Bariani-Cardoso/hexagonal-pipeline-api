using Adapter.InBound.HttpAdapter.DTOs.Xml;
using Domain.Models.Xml;

namespace Adapter.InBound.HttpAdapter.Mappers
{
    public static class XmlMapper
    {
        public static XmlResponse XmlDomainToResponseMapper(
            Xml xml)
        {
            ArgumentNullException.ThrowIfNull(xml);

            return new XmlResponse
            {
                TipoMovimento = xml.TipoMovimento,
                TipoNI = xml.TipoNI,
                NI = xml.NI,
                AnoMesCaixa = xml.AnoMesCaixa,
                NumeroRecibo = xml.NumeroRecibo,
                Id = xml.Id
            };
        }
    }
}
