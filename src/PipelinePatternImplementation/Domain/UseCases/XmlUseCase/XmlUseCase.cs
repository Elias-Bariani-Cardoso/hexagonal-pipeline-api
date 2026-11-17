using Domain.Models.Xml;
using Domain.Ports.InBound;
using Domain.Ports.OutBound;
using Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using PipelinePatternImplementation.Domain.ValueObjects;

namespace Domain.UseCases.XmlUseCase
{
    public class XmlUseCase : BaseUseCase, IXmlUseCase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IXmlRepository _xmlRepository;
        private readonly ILogger<XmlUseCase> _logger;

        public XmlUseCase(
            IUnitOfWork unitOfWork,
            IXmlRepository xmlRepository,
            ILogger<XmlUseCase> logger)
        {
            ArgumentNullException.ThrowIfNull(unitOfWork);
            ArgumentNullException.ThrowIfNull(xmlRepository);
            ArgumentNullException.ThrowIfNull(logger);

            _unitOfWork = unitOfWork;
            _xmlRepository = xmlRepository;
            _logger = logger;
        }

        public async Task<BaseReturn<List<Xml>>> ExecutarAsync(
            CancellationToken cancellationToken = default)
        {
            bool transactionStarted = false;

            try
            {
                await _unitOfWork.BeginTransactionAsync(
                    cancellationToken);

                transactionStarted = true;

                var registros = await _xmlRepository.ReadTop10(
                    cancellationToken);

                await _unitOfWork.CommitAsync(cancellationToken);

                transactionStarted = false;

                return new BaseReturn<List<Xml>>()
                    .Sucesso(registros);
            }
            catch (Exception ex)
            {
                if (transactionStarted)
                {
                    try
                    {
                        // O cancelamento da requisição não deve impedir
                        // a tentativa de rollback.
                        await _unitOfWork.RollbackAsync(
                            CancellationToken.None);
                    }
                    catch (Exception rollbackException)
                    {
                        _logger.LogError(
                            rollbackException,
                            "Falha ao reverter a transação de XML.");
                    }
                }

                if (ex is OperationCanceledException
                    && cancellationToken.IsCancellationRequested)
                {
                    throw;
                }

                _logger.LogError(
                    ex,
                    "Falha ao executar a consulta de XML.");

                return ErroSistema<List<Xml>>(ex, 5000);
            }
        }
    }
}