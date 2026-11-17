using Domain.Models.Xml;
using Domain.Ports.InBound;
using Domain.Ports.OutBound;
using Domain.ValueObjects;
using PipelinePatternImplementation.Domain.ValueObjects;

namespace Domain.UseCases.XmlUseCase
{
    public class XmlUseCase : BaseUseCase, IXmlUseCase
    {
        private readonly IXmlRepository _xmlRepository;

        public XmlUseCase(IXmlRepository xmlRepository)
        {
            ArgumentNullException.ThrowIfNull(xmlRepository);

            _xmlRepository = xmlRepository;
        }

        public async Task<BaseReturn<List<Xml>>> ExecutarAsync(
            CancellationToken cancellationToken = default)
        {
            try
            {
                var registros = await _xmlRepository.ReadTop10(
                    cancellationToken);

                return new BaseReturn<List<Xml>>().Sucesso(registros);
            }
            catch (OperationCanceledException)
                when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                return ErroSistema<List<Xml>>(ex, 5000);
            }
        }
    }
}