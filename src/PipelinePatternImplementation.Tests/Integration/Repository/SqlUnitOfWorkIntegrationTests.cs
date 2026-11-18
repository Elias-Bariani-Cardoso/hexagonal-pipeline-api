using Adapter.OutBound.SqlAdapter.Context;
using Adapter.OutBound.SqlAdapter.UnitOfWork;
using Domain.Models.Xml;
using Integration.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace Integration.Repository
{
    [Trait("Category", "Integration")]
    [Trait("Component", "SqlUnitOfWork")]
    public class SqlUnitOfWorkIntegrationTests :
        IClassFixture<SqlServerTestHelper>,
        IAsyncLifetime
    {
        private readonly SqlServerTestHelper _helper;

        private ServiceProvider? _provider;
        private AsyncServiceScope _scope;
        private bool _scopeCreated;

        private SqlContext _context = null!;
        private SqlUnitOfWork _unitOfWork = null!;

        public SqlUnitOfWorkIntegrationTests(
            SqlServerTestHelper helper)
        {
            _helper = helper;
        }

        public Task InitializeAsync()
        {
            _provider = _helper.CreateServiceProvider();

            _scope = _provider.CreateAsyncScope();
            _scopeCreated = true;

            _context = _scope.ServiceProvider
                .GetRequiredService<SqlContext>();

            _unitOfWork = _scope.ServiceProvider
                .GetRequiredService<SqlUnitOfWork>();

            return Task.CompletedTask;
        }

        [Fact]
        [Trait("Feature", "BeginTransaction")]
        public async Task CanBeginTransaction()
        {
            await _unitOfWork.BeginTransactionAsync();

            Assert.True(_unitOfWork.HasActiveTransaction);

            Assert.Same(
                _unitOfWork,
                _unitOfWork.TransactionContext);

            var efTransaction = _context.Database.CurrentTransaction;

            Assert.NotNull(efTransaction);

            Assert.Same(
                _unitOfWork.Transaction,
                efTransaction.GetDbTransaction());

            Assert.Equal(
                System.Data.IsolationLevel.ReadCommitted,
                _unitOfWork.Transaction.IsolationLevel);

            await _unitOfWork.RollbackAsync();

            AssertTransactionFinished();
        }

        [Fact]
        [Trait("Feature", "TransactionValidation")]
        public async Task CannotBeginTransactionWhenAlreadyActive()
        {
            await _unitOfWork.BeginTransactionAsync();

            var originalTransaction = _unitOfWork.Transaction;

            var exception = await Assert
                .ThrowsAsync<InvalidOperationException>(
                    () => _unitOfWork.BeginTransactionAsync());

            Assert.Equal(
                "Já existe uma transação ativa.",
                exception.Message);

            Assert.True(_unitOfWork.HasActiveTransaction);

            Assert.Same(
                originalTransaction,
                _unitOfWork.Transaction);

            await _unitOfWork.RollbackAsync();
        }

        [Fact]
        [Trait("Feature", "Commit")]
        public async Task CommitPersistsChanges()
        {
            var record = CreateRecord();

            await _unitOfWork.BeginTransactionAsync();

            _context.Set<TransactionTestRecord>().Add(record);

            // O próprio Unit of Work precisa chamar SaveChangesAsync.
            await _unitOfWork.CommitAsync();

            AssertTransactionFinished();

            await using var verificationContext =
                _helper.CreateContext();

            var persisted = await verificationContext
                .Set<TransactionTestRecord>()
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.Id == record.Id);

            Assert.NotNull(persisted);

            Assert.Equal(
                record.Description,
                persisted.Description);
        }

        [Fact]
        [Trait("Feature", "Rollback")]
        public async Task RollbackDiscardsSavedChanges()
        {
            var record = CreateRecord();

            await _unitOfWork.BeginTransactionAsync();

            _context.Set<TransactionTestRecord>().Add(record);

            // Executa o INSERT dentro da transação.
            await _context.SaveChangesAsync();

            Assert.True(
                await _context.Set<TransactionTestRecord>()
                    .AsNoTracking()
                    .AnyAsync(x => x.Id == record.Id));

            await _unitOfWork.RollbackAsync();

            AssertTransactionFinished();

            Assert.Empty(_context.ChangeTracker.Entries());

            await using var verificationContext =
                _helper.CreateContext();

            var exists = await verificationContext
                .Set<TransactionTestRecord>()
                .AsNoTracking()
                .AnyAsync(x => x.Id == record.Id);

            Assert.False(exists);
        }

        [Fact]
        [Trait("Feature", "Disposal")]
        public async Task DisposeDiscardsUncommittedChanges()
        {
            var record = CreateRecord();

            await _unitOfWork.BeginTransactionAsync();

            _context.Set<TransactionTestRecord>().Add(record);

            await _context.SaveChangesAsync();

            await _unitOfWork.DisposeAsync();

            AssertTransactionFinished();

            await using var verificationContext =
                _helper.CreateContext();

            var exists = await verificationContext
                .Set<TransactionTestRecord>()
                .AsNoTracking()
                .AnyAsync(x => x.Id == record.Id);

            Assert.False(exists);
        }

        [Fact]
        [Trait("Feature", "TransactionLifecycle")]
        public async Task CanBeginNewTransactionAfterCommit()
        {
            await _unitOfWork.BeginTransactionAsync();

            await _unitOfWork.CommitAsync();

            AssertTransactionFinished();

            await _unitOfWork.BeginTransactionAsync();

            AssertTransactionActive();

            await _unitOfWork.RollbackAsync();

            AssertTransactionFinished();
        }

        [Fact]
        [Trait("Feature", "TransactionLifecycle")]
        public async Task CanBeginNewTransactionAfterRollback()
        {
            await _unitOfWork.BeginTransactionAsync();

            await _unitOfWork.RollbackAsync();

            AssertTransactionFinished();

            await _unitOfWork.BeginTransactionAsync();

            AssertTransactionActive();

            await _unitOfWork.RollbackAsync();

            AssertTransactionFinished();
        }

        private void AssertTransactionActive()
        {
            Assert.True(_unitOfWork.HasActiveTransaction);

            var efTransaction = _context.Database.CurrentTransaction;

            Assert.NotNull(efTransaction);

            Assert.Same(
                _unitOfWork.Transaction,
                efTransaction.GetDbTransaction());
        }

        private void AssertTransactionFinished()
        {
            Assert.False(_unitOfWork.HasActiveTransaction);

            Assert.Null(_context.Database.CurrentTransaction);

            Assert.Throws<InvalidOperationException>(() =>
            {
                _ = _unitOfWork.Transaction;
            });

            Assert.Throws<InvalidOperationException>(() =>
            {
                _ = _unitOfWork.TransactionContext;
            });
        }

        private static TransactionTestRecord CreateRecord()
        {
            return new TransactionTestRecord
            {
                Id = Guid.NewGuid(),
                Description = "Registro para teste de transação."
            };
        }

        public async Task DisposeAsync()
        {
            try
            {
                if (_scopeCreated)
                {
                    _scopeCreated = false;

                    await _scope.DisposeAsync();
                }
            }
            finally
            {
                if (_provider is not null)
                {
                    await _provider.DisposeAsync();
                }
            }
        }
    }
}
