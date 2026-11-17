using Adapter.OutBound.SqlAdapter.Context;
using Domain.Ports.OutBound;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Data;
using System.Data.Common;

namespace Adapter.OutBound.SqlAdapter.UnitOfWork
{
    public sealed class SqlUnitOfWork : IUnitOfWork, ITransactionContext
    {
        private readonly SqlConnection _connection;
        private readonly SqlContext _context;

        private SqlTransaction? _transaction;

        public ITransactionContext TransactionContext
        {
            get
            {
                if (!HasActiveTransaction)
                {
                    throw new InvalidOperationException(
                        "Nenhuma transação está ativa. " +
                        "Chame BeginTransactionAsync antes de acessar TransactionContext.");
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

            _context = serviceProvider
                .GetRequiredService<SqlContext>();
        }

        public async Task BeginTransactionAsync(
            CancellationToken cancellationToken = default)
        {
            if (HasActiveTransaction)
            {
                throw new InvalidOperationException(
                    "Já existe uma transação ativa.");
            }

            if (_connection.State != ConnectionState.Open)
            {
                await _connection.OpenAsync(cancellationToken);
            }

            var transaction = (SqlTransaction)await _connection
                .BeginTransactionAsync(
                    IsolationLevel.ReadCommitted,
                    cancellationToken);

            try
            {
                // Inclui as operações do EF na transação SQL.
                await _context.Database.UseTransactionAsync(
                    transaction,
                    cancellationToken);

                _transaction = transaction;
            }
            catch
            {
                await transaction.DisposeAsync();
                throw;
            }
        }

        public async Task CommitAsync(
            CancellationToken cancellationToken = default)
        {
            var transaction = _transaction
                ?? throw new InvalidOperationException(
                    "Nenhuma transação está ativa.");

            // Persiste alterações rastreadas pelo EF dentro da transação.
            await _context.SaveChangesAsync(cancellationToken);

            // Confirma operações do EF e operações SQL que usaram
            // esta mesma transação.
            await transaction.CommitAsync(cancellationToken);

            await DisposeAsync();
        }

        public async Task RollbackAsync(
            CancellationToken cancellationToken = default)
        {
            var transaction = _transaction;

            if (transaction is null)
            {
                return;
            }

            try
            {
                await transaction.RollbackAsync(cancellationToken);
            }
            finally
            {
                // Remove estados em memória após a reversão.
                _context.ChangeTracker.Clear();

                await DisposeAsync();
            }
        }

        public async ValueTask DisposeAsync()
        {
            var transaction = _transaction;

            _transaction = null;

            if (transaction is null)
            {
                return;
            }

            try
            {
                // Desvincula o EF da transação externa.
                await _context.Database.UseTransactionAsync(
                    (DbTransaction?)null,
                    CancellationToken.None);
            }
            finally
            {
                await transaction.DisposeAsync();
            }
        }
    }
}