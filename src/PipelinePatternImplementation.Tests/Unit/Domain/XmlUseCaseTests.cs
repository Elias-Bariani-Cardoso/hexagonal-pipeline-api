using Domain.Enums;
using Domain.Models.Xml;
using Domain.Ports.OutBound;
using Domain.UseCases.XmlUseCase;
using Microsoft.Extensions.Logging;
using Moq;

namespace Unit.Domain
{
    [Trait("Category", "Unit")]
    [Trait("Component", "XmlUseCase")]
    public class XmlUseCaseTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWork;
        private readonly Mock<IXmlRepository> _repository;
        private readonly Mock<ILogger<XmlUseCase>> _logger;

        private readonly XmlUseCase _useCase;

        public XmlUseCaseTests()
        {
            _unitOfWork = new Mock<IUnitOfWork>();
            _repository = new Mock<IXmlRepository>();
            _logger = new Mock<ILogger<XmlUseCase>>();

            _unitOfWork
                .Setup(x => x.BeginTransactionAsync(
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _unitOfWork
                .Setup(x => x.CommitAsync(
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _unitOfWork
                .Setup(x => x.RollbackAsync(
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _repository
                .Setup(x => x.ReadTop10(
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Xml>());

            _useCase = new XmlUseCase(
                _unitOfWork.Object,
                _repository.Object,
                _logger.Object);
        }

        [Fact]
        [Trait("Feature", "Construction")]
        public void CanConstruct()
        {
            Assert.NotNull(_useCase);
        }

        [Fact]
        [Trait("Feature", "ArgumentValidation")]
        public void CannotConstructWithNullUnitOfWork()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new XmlUseCase(
                    null!,
                    _repository.Object,
                    _logger.Object));
        }

        [Fact]
        [Trait("Feature", "ArgumentValidation")]
        public void CannotConstructWithNullRepository()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new XmlUseCase(
                    _unitOfWork.Object,
                    null!,
                    _logger.Object));
        }

        [Fact]
        [Trait("Feature", "ArgumentValidation")]
        public void CannotConstructWithNullLogger()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new XmlUseCase(
                    _unitOfWork.Object,
                    _repository.Object,
                    null!));
        }

        [Fact]
        [Trait("Feature", "Execution")]
        public async Task ExecutarAsyncReturnsRepositoryRecords()
        {
            var expected = CreateRecords();

            using var cancellationSource =
                new CancellationTokenSource();

            var cancellationToken = cancellationSource.Token;

            _repository
                .Setup(x => x.ReadTop10(cancellationToken))
                .ReturnsAsync(expected);

            var result = await _useCase.ExecutarAsync(
                cancellationToken);

            Assert.Equal(EnumStatus.SUCESSO, result.Status);
            Assert.Same(expected, result.SuccessObject);

            _unitOfWork.Verify(
                x => x.BeginTransactionAsync(cancellationToken),
                Times.Once);

            _repository.Verify(
                x => x.ReadTop10(cancellationToken),
                Times.Once);

            _unitOfWork.Verify(
                x => x.CommitAsync(cancellationToken),
                Times.Once);

            VerifyNoRollback();

            _unitOfWork.VerifyNoOtherCalls();
            _repository.VerifyNoOtherCalls();
        }

        [Fact]
        [Trait("Feature", "Execution")]
        public async Task ExecutarAsyncReturnsEmptyListWhenRepositoryIsEmpty()
        {
            var expected = new List<Xml>();

            _repository
                .Setup(x => x.ReadTop10(
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            var result = await _useCase.ExecutarAsync();

            Assert.Equal(EnumStatus.SUCESSO, result.Status);

            Assert.NotNull(result.SuccessObject);
            Assert.Empty(result.SuccessObject);
            Assert.Same(expected, result.SuccessObject);

            _unitOfWork.Verify(
                x => x.CommitAsync(
                    It.IsAny<CancellationToken>()),
                Times.Once);

            VerifyNoRollback();
        }

        [Fact]
        [Trait("Feature", "ExecutionOrder")]
        public async Task ExecutarAsyncBeginsReadsAndCommitsInOrder()
        {
            var calls = new List<string>();
            var expected = CreateRecords();

            _unitOfWork
                .Setup(x => x.BeginTransactionAsync(
                    It.IsAny<CancellationToken>()))
                .Callback(() => calls.Add("Begin"))
                .Returns(Task.CompletedTask);

            _repository
                .Setup(x => x.ReadTop10(
                    It.IsAny<CancellationToken>()))
                .Callback(() => calls.Add("Read"))
                .ReturnsAsync(expected);

            _unitOfWork
                .Setup(x => x.CommitAsync(
                    It.IsAny<CancellationToken>()))
                .Callback(() => calls.Add("Commit"))
                .Returns(Task.CompletedTask);

            var result = await _useCase.ExecutarAsync();

            Assert.Equal(EnumStatus.SUCESSO, result.Status);

            Assert.Equal(
                new[] { "Begin", "Read", "Commit" },
                calls);

            VerifyNoRollback();
        }

        [Fact]
        [Trait("Feature", "BeginTransactionFailure")]
        public async Task ExecutarAsyncReturnsSystemErrorWhenBeginFails()
        {
            var exception = new InvalidOperationException(
                "Falha ao iniciar a transação.");

            _unitOfWork
                .Setup(x => x.BeginTransactionAsync(
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(exception);

            var result = await _useCase.ExecutarAsync();

            Assert.Equal(EnumStatus.SISTEMA, result.Status);
            Assert.NotNull(result.ErrorObject);

            _repository.Verify(
                x => x.ReadTop10(
                    It.IsAny<CancellationToken>()),
                Times.Never);

            VerifyNoCommit();
            VerifyNoRollback();

            VerifyErrorLogged(exception);
        }

        [Fact]
        [Trait("Feature", "RepositoryFailure")]
        public async Task ExecutarAsyncRollsBackWhenRepositoryFails()
        {
            var exception = new InvalidOperationException(
                "Falha na consulta de XML.");

            _repository
                .Setup(x => x.ReadTop10(
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(exception);

            var result = await _useCase.ExecutarAsync();

            Assert.Equal(EnumStatus.SISTEMA, result.Status);
            Assert.NotNull(result.ErrorObject);

            _unitOfWork.Verify(
                x => x.RollbackAsync(CancellationToken.None),
                Times.Once);

            VerifyNoCommit();

            VerifyErrorLogged(exception);
        }

        [Fact]
        [Trait("Feature", "CommitFailure")]
        public async Task ExecutarAsyncRollsBackWhenCommitFails()
        {
            var exception = new InvalidOperationException(
                "Falha ao confirmar a transação.");

            _unitOfWork
                .Setup(x => x.CommitAsync(
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(exception);

            var result = await _useCase.ExecutarAsync();

            Assert.Equal(EnumStatus.SISTEMA, result.Status);
            Assert.NotNull(result.ErrorObject);

            _repository.Verify(
                x => x.ReadTop10(
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _unitOfWork.Verify(
                x => x.RollbackAsync(CancellationToken.None),
                Times.Once);

            VerifyErrorLogged(exception);
        }

        [Fact]
        [Trait("Feature", "RollbackFailure")]
        public async Task ExecutarAsyncLogsBothErrorsWhenRollbackAlsoFails()
        {
            var repositoryException = new InvalidOperationException(
                "Falha na consulta.");

            var rollbackException = new InvalidOperationException(
                "Falha no rollback.");

            _repository
                .Setup(x => x.ReadTop10(
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(repositoryException);

            _unitOfWork
                .Setup(x => x.RollbackAsync(CancellationToken.None))
                .ThrowsAsync(rollbackException);

            var result = await _useCase.ExecutarAsync();

            Assert.Equal(EnumStatus.SISTEMA, result.Status);
            Assert.NotNull(result.ErrorObject);

            _unitOfWork.Verify(
                x => x.RollbackAsync(CancellationToken.None),
                Times.Once);

            VerifyNoCommit();

            VerifyErrorLogged(rollbackException);
            VerifyErrorLogged(repositoryException);
        }

        [Fact]
        [Trait("Feature", "Cancellation")]
        public async Task ExecutarAsyncRethrowsCancellationWhenBeginIsCanceled()
        {
            using var cancellationSource =
                new CancellationTokenSource();

            cancellationSource.Cancel();

            var cancellationToken = cancellationSource.Token;

            var exception = new OperationCanceledException(
                cancellationToken);

            _unitOfWork
                .Setup(x => x.BeginTransactionAsync(cancellationToken))
                .ThrowsAsync(exception);

            var actual = await Assert
                .ThrowsAsync<OperationCanceledException>(
                    () => _useCase.ExecutarAsync(cancellationToken));

            Assert.Same(exception, actual);

            _repository.Verify(
                x => x.ReadTop10(
                    It.IsAny<CancellationToken>()),
                Times.Never);

            VerifyNoCommit();
            VerifyNoRollback();

            VerifyNoErrorLogged();
        }

        [Fact]
        [Trait("Feature", "Cancellation")]
        public async Task ExecutarAsyncRollsBackAndRethrowsRepositoryCancellation()
        {
            using var cancellationSource =
                new CancellationTokenSource();

            var cancellationToken = cancellationSource.Token;

            var exception = new OperationCanceledException(
                cancellationToken);

            _repository
                .Setup(x => x.ReadTop10(cancellationToken))
                .Callback(() => cancellationSource.Cancel())
                .ThrowsAsync(exception);

            var actual = await Assert
                .ThrowsAsync<OperationCanceledException>(
                    () => _useCase.ExecutarAsync(cancellationToken));

            Assert.Same(exception, actual);

            _unitOfWork.Verify(
                x => x.RollbackAsync(CancellationToken.None),
                Times.Once);

            VerifyNoCommit();

            VerifyNoErrorLogged();
        }

        [Fact]
        [Trait("Feature", "Cancellation")]
        public async Task ExecutarAsyncRollsBackAndRethrowsCommitCancellation()
        {
            using var cancellationSource =
                new CancellationTokenSource();

            var cancellationToken = cancellationSource.Token;

            var exception = new OperationCanceledException(
                cancellationToken);

            _unitOfWork
                .Setup(x => x.CommitAsync(cancellationToken))
                .Callback(() => cancellationSource.Cancel())
                .ThrowsAsync(exception);

            var actual = await Assert
                .ThrowsAsync<OperationCanceledException>(
                    () => _useCase.ExecutarAsync(cancellationToken));

            Assert.Same(exception, actual);

            _unitOfWork.Verify(
                x => x.RollbackAsync(CancellationToken.None),
                Times.Once);

            VerifyNoErrorLogged();
        }

        [Fact]
        [Trait("Feature", "Cancellation")]
        public async Task ExecutarAsyncPreservesCancellationWhenRollbackFails()
        {
            using var cancellationSource =
                new CancellationTokenSource();

            var cancellationToken = cancellationSource.Token;

            var cancellationException =
                new OperationCanceledException(cancellationToken);

            var rollbackException = new InvalidOperationException(
                "Falha ao reverter após cancelamento.");

            _repository
                .Setup(x => x.ReadTop10(cancellationToken))
                .Callback(() => cancellationSource.Cancel())
                .ThrowsAsync(cancellationException);

            _unitOfWork
                .Setup(x => x.RollbackAsync(CancellationToken.None))
                .ThrowsAsync(rollbackException);

            var actual = await Assert
                .ThrowsAsync<OperationCanceledException>(
                    () => _useCase.ExecutarAsync(cancellationToken));

            Assert.Same(cancellationException, actual);

            VerifyErrorLogged(rollbackException);

            _logger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((state, type) => true),
                    It.IsAny<Exception>(),
                    It.IsAny<
                        Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            VerifyNoCommit();
        }

        [Fact]
        [Trait("Feature", "Cancellation")]
        public async Task ExecutarAsyncReturnsSystemErrorWhenTokenIsNotCanceled()
        {
            var exception = new OperationCanceledException(
                "Cancelamento sem solicitação no token recebido.");

            _repository
                .Setup(x => x.ReadTop10(
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(exception);

            var result = await _useCase.ExecutarAsync(
                CancellationToken.None);

            Assert.Equal(EnumStatus.SISTEMA, result.Status);
            Assert.NotNull(result.ErrorObject);

            _unitOfWork.Verify(
                x => x.RollbackAsync(CancellationToken.None),
                Times.Once);

            VerifyNoCommit();

            VerifyErrorLogged(exception);
        }

        private void VerifyNoCommit()
        {
            _unitOfWork.Verify(
                x => x.CommitAsync(
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        private void VerifyNoRollback()
        {
            _unitOfWork.Verify(
                x => x.RollbackAsync(
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        private void VerifyErrorLogged(Exception exception)
        {
            _logger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((state, type) => true),
                    It.Is<Exception?>(
                        loggedException =>
                            ReferenceEquals(
                                loggedException,
                                exception)),
                    It.IsAny<
                        Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        private void VerifyNoErrorLogged()
        {
            _logger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((state, type) => true),
                    It.IsAny<Exception>(),
                    It.IsAny<
                        Func<It.IsAnyType, Exception?, string>>()),
                Times.Never);
        }

        private static List<Xml> CreateRecords()
        {
            return new List<Xml>
            {
                new Xml
                {
                    Id = "1",
                    TipoMovimento = "1",
                    TipoNI = "1",
                    NI = "12345678901",
                    AnoMesCaixa = "202611",
                    NumeroRecibo = "RECIBO-1"
                },
                new Xml
                {
                    Id = "2",
                    TipoMovimento = "2",
                    TipoNI = "1",
                    NI = "98765432100",
                    AnoMesCaixa = "202611",
                    NumeroRecibo = "RECIBO-2"
                }
            };
        }
    }
}
