using Adapter.OutBound.SqlAdapter.Context;
using Adapter.OutBound.SqlAdapter.UnitOfWork;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Unit.Repository
{
    [Trait("Category", "Unit")]
    [Trait("Component", "SqlUnitOfWork")]
    public class SqlUnitOfWorkTests : IAsyncLifetime
    {
        private readonly Mock<IServiceProvider> _serviceProvider;
        private readonly Mock<SqlContext> _context;
        private readonly SqlConnection _connection;
        private readonly SqlUnitOfWork _unitOfWork;

        public SqlUnitOfWorkTests()
        {
            var options = new DbContextOptionsBuilder<SqlContext>()
                .Options;

            _context = new Mock<SqlContext>(options);

            // não abre conexão com o banco.
            _connection = new SqlConnection();

            _serviceProvider = new Mock<IServiceProvider>();

            _serviceProvider
                .Setup(x => x.GetService(typeof(SqlConnection)))
                .Returns(_connection);

            _serviceProvider
                .Setup(x => x.GetService(typeof(SqlContext)))
                .Returns(_context.Object);

            _unitOfWork = new SqlUnitOfWork(
                _serviceProvider.Object);
        }

        [Fact]
        [Trait("Feature", "Construction")]
        public void CanConstruct()
        {
            Assert.NotNull(_unitOfWork);

            _serviceProvider.Verify(
                x => x.GetService(typeof(SqlConnection)),
                Times.Once);

            _serviceProvider.Verify(
                x => x.GetService(typeof(SqlContext)),
                Times.Once);
        }

        [Fact]
        [Trait("Feature", "ArgumentValidation")]
        public void CannotConstructWithNullServiceProvider()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new SqlUnitOfWork(null!));
        }

        [Fact]
        [Trait("Feature", "DependencyResolution")]
        public void CannotConstructWithoutRegisteredSqlConnection()
        {
            var serviceProvider = new Mock<IServiceProvider>();

            var exception = Assert.Throws<InvalidOperationException>(
                () => new SqlUnitOfWork(serviceProvider.Object));

            Assert.Contains(
                nameof(SqlConnection),
                exception.Message);
        }

        [Fact]
        [Trait("Feature", "DependencyResolution")]
        public void CannotConstructWithoutRegisteredSqlContext()
        {
            var serviceProvider = new Mock<IServiceProvider>();

            serviceProvider
                .Setup(x => x.GetService(typeof(SqlConnection)))
                .Returns(_connection);

            var exception = Assert.Throws<InvalidOperationException>(
                () => new SqlUnitOfWork(serviceProvider.Object));

            Assert.Contains(
                nameof(SqlContext),
                exception.Message);
        }

        [Fact]
        [Trait("Feature", "Connection")]
        public void ConnectionReturnsRegisteredInstance()
        {
            var result = _unitOfWork.Connection;

            Assert.Same(_connection, result);
        }

        [Fact]
        [Trait("Feature", "TransactionState")]
        public void HasActiveTransactionReturnsFalseInitially()
        {
            Assert.False(_unitOfWork.HasActiveTransaction);
        }

        [Fact]
        [Trait("Feature", "TransactionValidation")]
        public void CannotGetTransactionWithoutActiveTransaction()
        {
            var exception = Assert.Throws<InvalidOperationException>(
                () =>
                {
                    _ = _unitOfWork.Transaction;
                });

            Assert.Equal(
                "Nenhuma transação está ativa.",
                exception.Message);
        }

        [Fact]
        [Trait("Feature", "TransactionValidation")]
        public void CannotGetTransactionContextWithoutActiveTransaction()
        {
            var exception = Assert.Throws<InvalidOperationException>(
                () =>
                {
                    _ = _unitOfWork.TransactionContext;
                });

            Assert.Equal(
                "Nenhuma transação está ativa. " +
                "Chame BeginTransactionAsync antes de acessar TransactionContext.",
                exception.Message);
        }

        [Fact]
        [Trait("Feature", "Commit")]
        public async Task CannotCommitWithoutActiveTransaction()
        {
            var exception = await Assert
                .ThrowsAsync<InvalidOperationException>(
                    () => _unitOfWork.CommitAsync());

            Assert.Equal(
                "Nenhuma transação está ativa.",
                exception.Message);

            Assert.False(_unitOfWork.HasActiveTransaction);

            _context.Verify(
                x => x.SaveChangesAsync(
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        [Trait("Feature", "Rollback")]
        public async Task RollbackWithoutActiveTransactionDoesNothing()
        {
            await _unitOfWork.RollbackAsync();

            Assert.False(_unitOfWork.HasActiveTransaction);

            _context.Verify(
                x => x.SaveChangesAsync(
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        [Trait("Feature", "Disposal")]
        public async Task CanDisposeWithoutActiveTransaction()
        {
            await _unitOfWork.DisposeAsync();

            Assert.False(_unitOfWork.HasActiveTransaction);

            // a conexão pertence ao escopo de DI, não ao Unit of Work.
            Assert.Same(
                _connection,
                _unitOfWork.Connection);
        }

        [Fact]
        [Trait("Feature", "Disposal")]
        public async Task CanDisposeMultipleTimesWithoutActiveTransaction()
        {
            await _unitOfWork.DisposeAsync();
            await _unitOfWork.DisposeAsync();

            Assert.False(_unitOfWork.HasActiveTransaction);
        }

        public Task InitializeAsync()
        {
            return Task.CompletedTask;
        }

        public async Task DisposeAsync()
        {
            await _unitOfWork.DisposeAsync();
            await _context.Object.DisposeAsync();
            await _connection.DisposeAsync();
        }
    }
}
