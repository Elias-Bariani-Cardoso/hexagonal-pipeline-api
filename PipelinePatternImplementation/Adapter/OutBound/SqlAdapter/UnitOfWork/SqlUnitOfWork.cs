using Domain.Ports.OutBound;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using System.Data;

namespace Adapter.OutBound.SqlAdapter.UnitOfWork
{
    public sealed class SqlUnitOfWork : IUnitOfWork, ITransactionContext
    {
        private readonly SqlConnection _connection;

        private SqlTransaction? _transaction;

        public ITransactionContext TransactionContext
        {
            get
            {
                if (!HasActiveTransaction)
                {
                    throw new InvalidOperationException(
                        "Nenhuma transação está ativa. Chame BeginTransactionAsync antes de acessar TransactionContext.");
                }

                return this;
            }
        }

        public IDbConnection Connection => _connection;

        public IDbTransaction Transaction =>
            _transaction
            ?? throw new InvalidOperationException(
                "Nenhuma transação está ativa.");

        public bool HasActiveTransaction =>
            _transaction is not null;

        public SqlUnitOfWork(IServiceProvider serviceProvider)
        {
            ArgumentNullException.ThrowIfNull(serviceProvider);

            _connection = serviceProvider
                .GetRequiredService<SqlConnection>();
        }

        public async Task BeginTransactionAsync(
            CancellationToken cancellationToken = default)
        {
            if (_transaction is not null)
            {
                throw new InvalidOperationException(
                    "Já existe uma transação ativa.");
            }

            if (_connection.State != ConnectionState.Open)
            {
                await _connection.OpenAsync(cancellationToken);
            }

            _transaction = (SqlTransaction)await _connection
                .BeginTransactionAsync(cancellationToken);
        }

        public async Task CommitAsync(
            CancellationToken cancellationToken = default)
        {
            if (_transaction is null)
            {
                throw new InvalidOperationException(
                    "Nenhuma transação está ativa.");
            }

            await _transaction.CommitAsync(cancellationToken);

            await DisposeAsync();
        }

        public async Task RollbackAsync(
            CancellationToken cancellationToken = default)
        {
            if (_transaction is null)
            {
                return;
            }

            try
            {
                await _transaction.RollbackAsync(cancellationToken);
            }
            finally
            {
                await DisposeAsync();
            }
        }

        public async ValueTask DisposeAsync()
        {
            var transaction = _transaction;

            _transaction = null;

            if (transaction is not null)
            {
                await transaction.DisposeAsync();
            }
        }
    }
}